using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using AudiocraticAPI.Models;
using CustomExtensions;

namespace AudiocraticAPI.Services
{
    public interface IHubSpotService
    {
        
        Task<dynamic> FetchDealByID(int ID, APIKey apiKey);
        Task<Contact> FetchContactByID(int ID, APIKey apiKey);
        Task<List<dynamic>> GetDealStagesAsync();
    }

    public class HubSpotService : IHubSpotService
    {
        public EventHandler DealFetched { get; set; }
        private readonly IAPIKeyService _apiKeyService;
        private readonly IDealService _dealService;

        public HubSpotService(IAPIKeyService apiKeyService, IDealService dealService)
        {
            _apiKeyService = apiKeyService;
            _dealService = dealService;
        }
        
        

        public async Task<dynamic> FetchDealByID(int ID, APIKey apiKey)
        {
            string url = "https://api.hubapi.com/deals/v1/deal/" + ID.ToString() + "?hapikey=" + apiKey.HubSpotKey;

            using(HttpClient client = new HttpClient())
            {
                Console.WriteLine("Requesting deal data from URL: " + url);

                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    string dealData = await response.Content.ReadAsStringAsync();
                    
                    dynamic responseDeal = null;

                    responseDeal = JsonConvert.DeserializeObject<ExpandoObject>(dealData);

                    return responseDeal;
                } 
            }
        }

        public async Task<Contact> FetchContactByID(int ID, APIKey apiKey) 
        {
            Contact contact = null;

            string url = "https://api.hubapi.com/contacts/v1/contact/vid/" + ID.ToString() + "/profile?hapikey=" + apiKey.HubSpotKey;

            using(HttpClient client = new HttpClient())
            {
                Console.WriteLine("Requesting contact data from URL: " + url);
                
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    string contactData = await response.Content.ReadAsStringAsync();

                    dynamic responseContact = JsonConvert.DeserializeObject<ExpandoObject>(contactData);

                    contact = MapHubSpotDataToContact(responseContact);

                    if(contact != null)
                    {
                        contact.HubSpotID = ID;
                        contact.UserId = apiKey.UserId;
                    }
                } 
            }

            return contact;
        }

        private Contact MapHubSpotDataToContact(dynamic data)
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

        public async Task<List<dynamic>> GetDealStagesAsync()
        {
            dynamic pipelines = 
                JsonConvert.DeserializeObject<dynamic>(await GetPipelinesWithDealStagesJsonAsync());
            
            List<dynamic> dealStages = new List<dynamic>();

            foreach(dynamic pipeline in pipelines.results)
            {
                foreach(dynamic dealStage in pipeline.stages)
                {
                    dealStage.pipelineId = pipeline.pipelineId;
                    dealStage.pipelineLabel = pipeline.label;
                    
                    dealStages.Add(dealStage);
                }
            }

            return dealStages;
        }

        private async Task<string> GetPipelinesWithDealStagesJsonAsync()
        {
            string result = string.Empty;

            APIKey apiKey = await _apiKeyService.GetAPIKeyAsync();

            string url = 
                "https://api.hubapi.com/crm-pipelines/v1/pipelines/deals?hapikey=" + apiKey.HubSpotKey;

            using(HttpClient client = new HttpClient())
            {
                using(HttpResponseMessage response = await client.GetAsync(url))
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }
    }
}




    