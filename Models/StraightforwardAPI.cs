using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AudiocraticAPI.Models;


namespace AudiocraticAPI.Models
{
    public class DealStageChange
    {       
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public DateTime ChangeDateTime { get; set; }
        public int DealID { get; set; }
        public Deal Deal { get; set; }

    }

    public class Deal
    {
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public int HubSpotID { get; set; }
        public string Name { get; set; }
        public string StageID { get; set; }
        public List<Contact> Contacts { get; set; }
    }

    public class DealStageFilter
    {
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public string StageID { get; set; }
        public string StageName { get; set; }
    }

    public class Contact
    {
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public DateTime LastModified { get; set; }
        public int? DealID { get; set; }
        public Deal Deal { get; set; }
        public int? HubSpotID { get; set; }
        public string ConstantContactID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Type { get; set; }
        public List<ContactEmail> EmailAddresses { get; set; }
        
        [NotMapped]
        public List<ContactList> ContactLists { get; set; }
    }

    public class ContactList
    {
        public string ListID { get; set; }
        public string ListName { get; set; }
    }

    public class ContactEmail
    {
        public int ID { get; set; }
        public int ContactID { get; set; }
        public Contact Contact { get; set; }
        public string Address { get; set; }
    }

    public class ContactListAddLog
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public DateTime AddedDateTime { get; set; }
        public int ContactID { get; set; }
        public Contact Contact { get; set; }
        public string ListID { get; set; }
        public string ListName { get; set; }
    }

    public class ContactTypeToListRelationship
    {
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        [Required]
        public string TypeName { get; set; }
        [Required]
        public string ListID { get; set; }
        [Required]
        public string ListName { get; set; }
    }

    public class APIKey
    {
        public int ID { get; set; }
        public IdentityUser User { get; set; }
        public string UserId { get; set; }
        public string Key { get; set; }
        public string HubSpotKey { get; set; }
        public string ConstantContactPublicKey { get; set; }
        public string ConstantContactPrivateKey { get; set; }
    }

}
