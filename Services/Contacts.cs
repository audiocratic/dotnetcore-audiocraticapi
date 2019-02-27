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
using CustomExtensions;

namespace AudiocraticAPI.Services
{
    public interface IContactService
    {
        dynamic MapContactToConstantContactData(Contact contact);
        dynamic MapContactToConstantContactData(
            Contact contact, 
            IDictionary<string, dynamic> customProperties);
        dynamic MapContactToConstantContactData(
            Contact contact,
            dynamic data
        );  
        dynamic MapContactToConstantContactData(
            Contact contact, 
            dynamic data,
            IDictionary<string, dynamic> customProperties);
        
        Contact MapConstantContactDataToContact(dynamic data);
        Contact MapHubSpotDataToContact(dynamic data);
        Task<Contact> GetMostRecentContactVersion(Contact contact, APIKey apiKey);
    }

    public class ContactService : IContactService
    {
        public readonly APIContext _context;

        public ContactService(APIContext apiContext)
        {
            _context = apiContext;
        }
        
        ///<summary>Used primarily for adding data for a new contact</summary>
        public dynamic MapContactToConstantContactData(Contact contact)
        {
            return MapContactToConstantContactData(
                contact, 
                null, 
                (IDictionary<string, dynamic>)null);
        }

         ///<summary>Used primarily for adding data for a new contact</summary>
        public dynamic MapContactToConstantContactData(
            Contact contact, 
            IDictionary<string, dynamic> customProperty)
        {            
            return MapContactToConstantContactData(contact, null, customProperty);
        }

        ///<summary>Used primarily for updating an existing contact</summary>
        public dynamic MapContactToConstantContactData(
            Contact contact,
            dynamic data
        )
        {
            return MapContactToConstantContactData(contact, data, null);
        }

        ///<summary>Used primarily for updating an existing contact</summary>
        public dynamic MapContactToConstantContactData(
            Contact contact, 
            dynamic data, 
            IDictionary<string, dynamic> customProperties)
        {
            if(data == null) 
            {
                data = new ExpandoObject();
                data.id = contact.ConstantContactID; 
            }

            //Overwrite properties
            data.first_name = contact.FirstName;
            data.last_name = contact.LastName;
            data.job_title = (contact.Type.Length > 50 ? contact.Type.Substring(0, 50) : contact.Type);

            //Merge e-mails and contact lists
            data = MergeConstantContactDataEmailAddresses(contact, data);
            data = MergeConstantContactDataLists(contact, data);
            data = ApplyCustomPropertiesToConstantContactData(data, customProperties);

            return data;
        }

        ///<summary>Merge contact lists on contact instance with 
        ///lists on JSON Constant Contact contact data.</summary>
        private dynamic MergeConstantContactDataLists(Contact contact, dynamic data)
        {
            //Merge lists (add lists based on new relationships to Constant Contact)
            List<ExpandoObject> mergedLists = new List<ExpandoObject>();

            foreach(ContactList list in contact.ContactLists)            
            {
                int i = 0;

                //Check if the contact already has this list
                //if(DynamicHasKey("lists", data))
                if(((IDictionary<string, object>)data).ContainsKey("lists"))
                {
                    foreach(dynamic constantContactList in data.lists)
                    {
                        if(constantContactList.id == list.ListID)
                        {
                            mergedLists.Add(constantContactList);
                            i++;
                        }
                    }
                }

                //If they don't have it
                if(i == 0)
                {
                    dynamic listToAdd = new ExpandoObject();
                    listToAdd.id = list.ListID;
                    mergedLists.Add(listToAdd);
                }
            }

            data.lists = mergedLists;
            
            return data;
        }

        private dynamic MergeConstantContactDataEmailAddresses(Contact contact, dynamic data)
        {
            //Merge e-mails (add non-existing contact e-mails to Constant Contact data)
            List<ExpandoObject> mergedAddresses = new List<ExpandoObject>();
            
            foreach(ContactEmail contactEmail in contact.EmailAddresses)
            {
                int i = 0;

                if(((IDictionary<string, object>)data).ContainsKey("email_addresses"))
                {
                    foreach(dynamic constantContactEmail in data.email_addresses)
                    {
                        if(contactEmail.Address == constantContactEmail.email_address)
                        {
                            mergedAddresses.Add(constantContactEmail);
                            i++;
                        } 
                    }
                }
                
                if(i == 0)
                {
                    dynamic address = new ExpandoObject();
                    address.email_address = contactEmail.Address;
                    mergedAddresses.Add(address);
                }  
            }
            
            data.email_addresses = mergedAddresses;
            
            return data;
        }

        public dynamic ApplyCustomPropertiesToConstantContactData(
            dynamic data,
            IDictionary<string, dynamic> customProperties
        )
        {
            if(customProperties != null)
            {
                foreach(KeyValuePair<string, dynamic> propEntry in customProperties)
                {
                    dynamic prop = ((List<dynamic>)data.custom_fields)
                        .FirstOrDefault(f => (string)f.name == propEntry.Key);
                    
                    if(prop == null)
                    {
                        prop = new ExpandoObject();

                        prop.name = propEntry.Key;
                        prop.value = propEntry.Value;
                        
                        ((List<object>)data.custom_fields).Add(prop);
                    }
                    else
                    {
                        prop.value = propEntry.Value;
                    }
                }
            }
            
            return data;
        }

        public Contact MapConstantContactDataToContact(dynamic contactData)
        {
            Contact contact = new Contact();

            contact.ConstantContactID = contactData.id;
            contact.FirstName = contactData.first_name;
            contact.LastName = contactData.last_name;
            contact.Type = contactData.job_title;

            contact.ContactLists = new List<ContactList>();

            foreach(dynamic list in contactData.lists)
            {
                ContactList contactList = new ContactList();
                contactList.ListID = list.id;
                contact.ContactLists.Add(contactList);
            }

            contact.EmailAddresses = new List<ContactEmail>();

            foreach(dynamic email in contactData.email_addresses)
            {
                ContactEmail contactEmail = new ContactEmail();
                contactEmail.Address = email.email_address;
                contact.EmailAddresses.Add(contactEmail);
            }

            return contact;
        }

        public Contact MapHubSpotDataToContact(dynamic data)
        {
            Contact contact = null;

            if(((ExpandoObject)data.properties).HasProperty("contact_type")
                && ((ExpandoObject)data.properties).HasProperty("email"))
            {
                contact = new Contact();
                
                contact.EmailAddresses = new List<ContactEmail>();
                contact.EmailAddresses.Add(new ContactEmail(){
                    Address = data.properties.email.value
                });

                contact.Type = data.properties.contact_type.value;
                
                if(((ExpandoObject)data.properties).HasProperty("firstname"))
                    contact.FirstName = data.properties.firstname.value;

                if(((ExpandoObject)data.properties).HasProperty("lastname"))
                    contact.LastName = data.properties.lastname.value;
            }

            return contact;
        }

        public async Task<Contact> GetMostRecentContactVersion(Contact contact, APIKey key)
        {
            return await _context.Contacts
                .Include(c => c.User)
                .Include(c => c.EmailAddresses)
                .Include(c => c.ContactLists)
                .OrderByDescending(c => c.LastModified)
                .Where(c => c.UserId == key.UserId && c.ConstantContactID != null)
                .FirstOrDefaultAsync(c => c.HubSpotID == contact.HubSpotID);
        }
    }
}