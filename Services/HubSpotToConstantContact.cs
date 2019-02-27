using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;
using CustomExtensions;

namespace AudiocraticAPI.Services
{
    public interface IHubSpotToConstantContactService
    {
        Task<IActionResult> TransferContactsFromHubSpotToConstantContact(string requestJSON, APIKey apiKey);
    }

    public class HubSpotToConstantContactService : IHubSpotToConstantContactService
    {
        private readonly IHubSpotService _hubSpotService;
        private readonly APIContext _context;
        private readonly IConstantContactService _constantContactService;
        private readonly HubSpotToCCIntegrationLogService _logService;
        private readonly IDealService _dealService;
        private readonly IContactService _contactService;

        public HubSpotToConstantContactService(
            IHubSpotService hubSpotService,
            IDealService dealService,
            IConstantContactService constantContactService,
            IContactService contactService,
            APIContext context,
            HubSpotToCCIntegrationLogService logService
        )
        {
            _hubSpotService = hubSpotService;
            _dealService = dealService;
            _constantContactService = constantContactService;
            _contactService = contactService;
            _context = context;
            _logService = logService;
        }
        
        //GENERAL FLOW:
        // 1) Receive deal change event from HubSpot
        // 2) Grab corresponding deal along with contacts
        //   a) Need to pass deal name along with contact into 'Project' field in Constant Contact
        // 3) For each contact
        //   a) Pull current info from HubSpot
        //   b) Validate
        //       i) Should have at least one e-mail address
        //       ii) Should have a contact type (custom field in both HubSpot and Constant Contact)
        //       iii) Contact type should have associated lists
        //   c) Compare current HubSpot data to most recent data in database to determine what kind
        //      of operation is needed
        //       i) If the same, do nothing
        //       ii) If e-mail addresses or eligible contact lists have changed, update
        //       iii) If not in the database, add to Constant Contact (verify doesn't exist first)


        public async Task<IActionResult> TransferContactsFromHubSpotToConstantContact(
            string requestJSON,
            APIKey apiKey
        )
        {
            //Verify deal is in proper stage
            dynamic eventData = JsonConvert.DeserializeObject<List<ExpandoObject>>(requestJSON);

            if(eventData[0] == null 
                || !((ExpandoObject)eventData[0]).HasProperty("propertyName"))
                throw new Exception("Unable to parse request body.");

            if(eventData[0].propertyName.ToString() != "dealstage")
                throw new Exception("Ignoring event. Property changed is not dealstage.");

            if(!await DealStageIsValid(eventData[0].propertyValue.ToString())) 
                throw new Exception("Invalid dealstage. Ignoring deal.");

            return await TransferContactsFromHubSpotToConstantContact((int)eventData[0].objectId, apiKey);
        }

        public async Task<IActionResult> TransferContactsFromHubSpotToConstantContact(
            int dealHubSpotId, 
            APIKey apiKey)
        {
            //Fetch contacts from deal
            Deal deal = await FetchDealWithContactsFromHubSpot(dealHubSpotId, apiKey);

            foreach(Contact hubspotContact in deal.Contacts)
            {
                hubspotContact.LastModified = DateTime.Now;
                
                await FetchContactListsForContact(hubspotContact);

                Dictionary<string, dynamic> customProps = new Dictionary<string, dynamic>();
                customProps.Add("custom_field_1", deal.Name);

                if(ContactCanBeTransferred(hubspotContact))
                {
                    //Has this contact been transferred before
                    Contact loggedContact = 
                        await _contactService.GetMostRecentContactVersion(hubspotContact, apiKey);
                    
                    if(loggedContact == null)
                    {
                        //Check if contact can be located in ConstantContact by e-mail address
                        dynamic constantContactContact = 
                            await GetContactDataByEmail(hubspotContact, apiKey);
                        
                        if(constantContactContact == null) //If doesn't exist, create new
                        {
                            constantContactContact =
                                _contactService.MapContactToConstantContactData(hubspotContact, customProps);

                            Console.WriteLine("Adding contact to Constant Contact.");
                            
                            constantContactContact = 
                                await _constantContactService.AddContact(constantContactContact, apiKey);

                            hubspotContact.ConstantContactID = constantContactContact.id;
                        }
                        else //If does exist, update
                        {
                            hubspotContact.ConstantContactID = constantContactContact.id;
                            
                            _contactService.MapContactToConstantContactData(
                                hubspotContact, constantContactContact, customProps);

                            Console.WriteLine("Updating contact in Constant Contact.");

                            await _constantContactService.UpdateContact(constantContactContact, apiKey);
                        }

                        Console.WriteLine(JsonConvert.SerializeObject(constantContactContact));
                    }
                    else if(ContactHasChanged(hubspotContact, loggedContact))
                    {
                        dynamic constantContactContact = 
                            await _constantContactService.FetchContactDataByID(hubspotContact.ConstantContactID, apiKey);
                        
                        _contactService.MapContactToConstantContactData(
                            hubspotContact, constantContactContact, customProps);

                        await _constantContactService.UpdateContact(constantContactContact, apiKey);
                    }
                    else
                    {
                        Console.WriteLine("Contact has not changed. Skipping.");
                    }
                }
            }
            
            await _logService.Save(deal, apiKey);

            return _logService.GetDealStageChangeJson();
        }

        public async Task<dynamic> GetContactDataByEmail(Contact contact, APIKey apiKey)
        {
            dynamic data = null;

            foreach(ContactEmail email in contact.EmailAddresses)
            {
                if(data == null)
                {
                    //Attempt to fetch Constant Contact contact
                    data = 
                        await _constantContactService.FetchContactDataByEmail(email.Address, apiKey);
                }
            }

            return data;
        }

        public async Task<Deal> FetchDealWithContactsFromHubSpot(int dealID, APIKey apiKey)
        {
            Deal deal = null;

            dynamic responseDeal = await _hubSpotService.FetchDealDataByID(dealID, apiKey);

            if(responseDeal == null) throw new Exception("Unable to retrieve HubSpot deal.");

            deal = _dealService.GetDealFromHubSpotDealData(responseDeal, apiKey);

            deal.Contacts = new List<Contact>();

            if(((ExpandoObject)responseDeal).HasProperty("associations.associatedVids"))
            {
                foreach(dynamic i in responseDeal.associations.associatedVids)
                {
                    Contact contact = await _hubSpotService.FetchContactByID((int)i, apiKey);
                    
                    if(contact != null)
                    {
                        deal.Contacts.Add(contact);
                    }
                }
            }

            Console.WriteLine(JsonConvert.SerializeObject(deal));
            return deal;
        }

        public async Task<bool> DealStageIsValid(string dealStage)
        {
            int dealStageCount = await _context.DealStageFilters
                .Where(d => d.StageID == dealStage)
                .CountAsync();

            return (dealStageCount > 0);            
        }

        public async Task FetchContactListsForContact(Contact contact)
        {
            //Fetch lists to which this contact should be added
                contact.ContactLists = new List<ContactList>();

                string[] types = contact.Type.Split(";");

                List<ContactList> contactLists = new List<ContactList>();

                foreach(string type in types)
                {
                    contactLists.AddRange(await FetchContactListsByContactType(type));
                }

                foreach(ContactList contactList in contactLists)
                {
                    ContactList list = new ContactList();
                    list.ListID = contactList.ListID;
                    list.ListName = contactList.ListName;
                    contact.ContactLists.Add(list);
                }
        }

        public async Task<List<ContactList>> FetchContactListsByContactType(string contactType)
        {
            List<ContactTypeToListRelationship> relationships = await _context.ContactTypeToListRelationship
                .Where(c => c.TypeName == contactType)
                .ToListAsync();

            return relationships
                .Select(r => 
                    {
                        return new ContactList()
                        {
                            ListID = r.ListID,
                            ListName = r.ListName
                        };
                    }).ToList();
        }

        private bool ContactCanBeTransferred(Contact contact)
        {
            bool transferable = true;

            if(contact.EmailAddresses?.Count < 1) transferable = false;
            if(contact.Type == null || contact.Type == string.Empty) transferable = false;
            if(contact.ContactLists?.Count < 1) transferable = false;
            
            return transferable;
        }

        private bool ContactHasChanged(Contact hubspotContact, Contact dbContact)
        {
            bool changed = false;

            //If the contact from HubSpot has more e-mail addresses, it has changed
            if(hubspotContact.EmailAddresses.Count > dbContact.EmailAddresses.Count) changed = true;
            
            //Check if e-mail addresses line up
            if(!changed)
            {
                foreach(ContactEmail address in hubspotContact.EmailAddresses)
                {
                    if(!changed)
                    {
                        if(dbContact.EmailAddresses.FirstOrDefault(a => a.Address == address.Address) == null)
                            changed = true;
                    }
                }
            }

            //Check if lists line up
            if(!changed)
            {
                foreach(ContactList list in hubspotContact.ContactLists)
                {
                    if(!changed)
                    {
                        if(dbContact.ContactLists.FirstOrDefault(l => l.ListID == list.ListID) == null)
                            changed = true;
                    }
                }
            }

            return changed;
        }
    }

    public class HubSpotToCCIntegrationLogService
    {
        private DealStageChange DealStageChange { get; set; }
        private readonly APIContext _context;

        public HubSpotToCCIntegrationLogService(APIContext context)
        {
            _context = context;
        }

        public async Task Save(Deal deal, APIKey apiKey)
        {
            DealStageChange = new DealStageChange();
            
            DealStageChange.UserId = apiKey.UserId;
            DealStageChange.Deal = deal;
            DealStageChange.Deal.UserId = apiKey.UserId;
            DealStageChange.ChangeDateTime = DateTime.Now;
            
            _context.DealStageChanges.Add(DealStageChange);

            await _context.SaveChangesAsync();
        }

        //Return a JsonResult that has any user data stripped
        public JsonResult GetDealStageChangeJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.ContractResolver = new DealStageChangeContractResolver();
            
            return new JsonResult(DealStageChange, settings);
        }

        public async Task<List<DealStageChange>> GetDealStageChangesAsync(string userName)
        {
            return await _context.DealStageChanges
                .Include(d => d.User)
                .Include(d => d.Deal)
                    .ThenInclude(d => d.Contacts)
                        .ThenInclude(c => c.EmailAddresses)
                .Include(d => d.Deal)
                    .ThenInclude(d => d.Contacts)
                        .ThenInclude(c => c.ContactLists)
                .Where(d => d.User.UserName == userName)
                .OrderByDescending(d => d.ChangeDateTime)
                .ToListAsync();
        }

        protected class DealStageChangeContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties (
                Type type, 
                MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                properties =
                    properties
                        .Where(p => p.PropertyType.Name != "IdentityUser" && p.PropertyName != "UserId")
                            .ToList();

                return properties;
            }
        }
    }
}

