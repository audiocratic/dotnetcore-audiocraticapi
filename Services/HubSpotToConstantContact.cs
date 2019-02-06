using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            _contactService.NewListAddedToContact += ConstantContactService_ContactListAdded;
            _logService = logService;
        }


        //TODO:
        // - Migrate database (added logging flexibility)
        
        //GENERAL FLOW:
        // 1) Receive deal change event from HubSpot
        // 2) Grab corresponding deal along with contacts
        //   a) Need to pass deal name along with contact into 'Project' field in Constant Contact
        // 3) For each contact
        //   a) Pull current info from HubSpot
        //   b) Validate
        //       i) Should have at least one e-mail address
        //       ii) Should have a contact type (custom field in both HubSpot and Constant Contact)
        //   c) Compare current HubSpot data to most recent data in database to determine what kind
        //      of operation is needed
        //       i) If the same, do nothing
        //       ii) If e-mail addresses or eligible contact lists have changed, update
        //       iii) If not in the database, add to Constant Contact (verify doesn't exist first)


        public async Task<IActionResult> TransferContactsFromHubSpotToConstantContact(
            string requestJSON, 
            APIKey apiKey)
        {
            //Verify deal is in proper stage
            dynamic eventData = JsonConvert.DeserializeObject<ExpandoObject>(requestJSON);

            if(eventData[0] == null 
                || !((ExpandoObject)eventData[0]).HasProperty("propertyName"))
                throw new Exception("Unable to parse request body.");

            if(eventData[0].propertyName.ToString() != "dealstage")
                throw new Exception("Ignoring event. Property changed is not dealstage.");

            if(!await DealStageIsValid(eventData[0].propertyValue.ToString())) 
                throw new Exception("Invalid dealstage. Ignoring deal.");

            //Fetch contacts from deal
            Deal deal = await FetchDealFromHubSpot((int)eventData[0].objectId, apiKey);

            //Begin logging
            _logService.DealStageChange = new DealStageChange();
            _logService.DealStageChange.UserId = apiKey.UserId;
            _logService.DealStageChange.ChangeDateTime = DateTime.Now;
            _logService.DealStageChange.Deal = deal;

            //TODO: Add filter for validating contacts here:
            //  - Contact type
            //  - At least one e-mail
            foreach(Contact hubspotContact in deal.Contacts)
            {
                //Fetch lists to which this contact should be added
                hubspotContact.ContactLists = new List<ContactList>();

                string[] types = hubspotContact.Type.Split(";");

                List<string> relationships = new List<string>();

                foreach(string type in types)
                {
                    relationships.AddRange(await FetchContactListsByContactType(type));
                }

                foreach(string relationship in relationships)
                {
                    ContactList list = new ContactList();
                    list.ListID = relationship;
                    hubspotContact.ContactLists.Add(list);
                }

                dynamic constantContactContact = null;
                
                foreach(ContactEmail email in hubspotContact.EmailAddresses)
                {
                    if(constantContactContact == null)
                    {
                        //Attempt to fetch Constant Contact contact
                        constantContactContact = 
                            await _constantContactService.FetchContactDataByEmail(email.Address, apiKey);
                    }
                }
                
                if(constantContactContact == null) //If doesn't exist, create new
                {
                    constantContactContact =
                        _contactService.MapContactToConstantContactData(hubspotContact);
                    
                    if(constantContactContact.lists.Count > 0)
                    {
                        Console.WriteLine("Adding contact to Constant Contact.");
                    
                        await _constantContactService.AddContact(constantContactContact, apiKey);
                    }
                }
                else //If does exist, update
                {
                    _contactService.MapContactToConstantContactData(
                        hubspotContact, constantContactContact);

                    Console.WriteLine("Updating contact in Constant Contact.");

                    await _constantContactService.UpdateContact(constantContactContact, apiKey);
                }

                Console.WriteLine(JsonConvert.SerializeObject(constantContactContact));
            }
            
            await _logService.Save(apiKey);


            return new JsonResult(_logService.DealStageChange);
        }

        public async Task<Deal> FetchDealFromHubSpot(int dealID, APIKey apiKey)
        {
            Deal deal = null;

            dynamic responseDeal = await _hubSpotService.FetchDealByID(dealID, apiKey);

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

        public async Task<List<string>> FetchContactListsByContactType(string contactType)
        {
            List<ContactTypeToListRelationship> relationships = await _context.ContactTypeToListRelationship
                .Where(c => c.TypeName == contactType)
                .ToListAsync();

            return relationships.Select(r => r.ListID).ToList();
        }

        private bool ContactCanBeTransferred()
        {
            return false;
        }

        private void ConstantContactService_ContactListAdded(
            object sender, 
            EventArgs e)
        {
            NewListAddedToContactEventArgs args = (NewListAddedToContactEventArgs)e;

            ContactListAddLog log = new ContactListAddLog();
            log.Contact = args.Contact;
            log.ListID = args.ContactList.ListID;
            log.ListName = args.ContactList.ListName;

            _logService.ListsAdded = new List<ContactListAddLog>();
            _logService.ListsAdded.Add(log);
        }
    }

    public class HubSpotToCCIntegrationLogService
    {
        public DealStageChange DealStageChange { get; set; }
        public List<ContactListAddLog> ListsAdded { get; set; }
        private readonly APIContext _context;

        public HubSpotToCCIntegrationLogService(APIContext context)
        {
            _context = context;
        }

        public async Task Save(APIKey apiKey)
        {
            DealStageChange.UserId = apiKey.UserId;
            DealStageChange.Deal.UserId = apiKey.UserId;

            if(ListsAdded != null)
            {
                ListsAdded.ForEach(log => {
                    log.UserId = apiKey.UserId;
                });

                _context.ContactListAddLogs.AddRange(ListsAdded);
            }
            
            _context.DealStageChanges.Add(DealStageChange);

            await _context.SaveChangesAsync();
        }
    }
}

