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
        EventHandler NewListAddedToContact { get; set; }
        dynamic MapContactToConstantContactData(Contact contact);
        dynamic MapContactToConstantContactData(Contact contact, dynamic data);
    }

    public class ContactService : IContactService
    {
        public EventHandler NewListAddedToContact { get; set; }
        
        public dynamic MapContactToConstantContactData(Contact contact)
        {
            return MapContactToConstantContactData(contact, null);
        }

        public dynamic MapContactToConstantContactData(Contact contact, dynamic data)
        {
            if(data == null) 
            {
                data = new ExpandoObject();
                data.id = contact.ConstantContactID; 
            }
                
            data.first_name = contact.FirstName;
            data.last_name = contact.LastName;
            data.job_title = (contact.Type.Length > 50 ? contact.Type.Substring(0, 50) : contact.Type);

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

                    NewListAddedToContactEventArgs args = new NewListAddedToContactEventArgs();
                    args.ContactList = list;
                    args.Contact = contact;
                    NewListAddedToContact?.Invoke(this, args);
                }
            }

            data.lists = mergedLists;

            return data;
        }

        private Contact MapConstantContactDataToContact(dynamic contactData)
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
    }

    public class NewListAddedToContactEventArgs : EventArgs
    {
        public ContactList ContactList { get; set; }
        public Contact Contact { get; set; }
    }
}