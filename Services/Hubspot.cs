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
        
        Task<dynamic> FetchDealDataByID(int ID, APIKey apiKey);
        Task<Contact> FetchContactByID(int ID, APIKey apiKey);
        Task<List<dynamic>> GetDealStagesAsync(APIKey apiKey);
        Task<dynamic> FetchContactDataByID(int ID, APIKey apiKey);
    }

    public class HubSpotService : IHubSpotService
    {
        public EventHandler DealFetched { get; set; }
        private readonly IDealService _dealService;
        private readonly IContactService _contactService;

        public HubSpotService(
            IDealService dealService,
            IContactService contactService)
        {
            _dealService = dealService;
            _contactService = contactService;
        }
        
        public async Task<dynamic> FetchDealDataByID(int ID, APIKey apiKey)
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

            dynamic responseContact = await FetchContactDataByID(ID, apiKey);

            contact = _contactService.MapHubSpotDataToContact(responseContact);

            if(contact != null)
            {
                contact.HubSpotID = ID;
                contact.UserId = apiKey.UserId;
            }
                
            return contact;
        }

        public async Task<dynamic> FetchContactDataByID(int ID, APIKey apiKey)
        {
            string url = "https://api.hubapi.com/contacts/v1/contact/vid/" + ID.ToString() + "/profile?hapikey=" + apiKey.HubSpotKey;

            using(HttpClient client = new HttpClient())
            {
                Console.WriteLine("Requesting contact data from URL: " + url);
                
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    string contactData = await response.Content.ReadAsStringAsync();

                    dynamic responseContact = JsonConvert.DeserializeObject<ExpandoObject>(contactData);

                    return responseContact;
                } 
            }
        }

        public async Task<List<dynamic>> GetDealStagesAsync(APIKey apiKey)
        {
            dynamic pipelines = 
                JsonConvert.DeserializeObject<dynamic>(await GetPipelinesWithDealStagesJsonAsync(apiKey));
            
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

        private async Task<string> GetPipelinesWithDealStagesJsonAsync(APIKey apiKey)
        {
            string result = string.Empty;

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




    