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
using Newtonsoft.Json.Linq;
using AudiocraticAPI.Models;

namespace AudiocraticAPI.Services
{
    public interface IConstantContactService
    {
        
        Task<dynamic> FetchContactListsAsync();
        Task<dynamic> FetchContactDataByEmail(string address, APIKey apiKey);
        Task UpdateContact(dynamic contact, APIKey apiKey);
        Task AddContact(dynamic contact, APIKey apiKey);
    }

    public class ConstantContactService : IConstantContactService
    {
        public dynamic APIKeys { get; set; }

        private readonly IAPIKeyService _apiKeyService;

        

        public ConstantContactService(IAPIKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        public async Task<dynamic> FetchContactListsAsync()
        {
            return JsonConvert.DeserializeObject<dynamic>(await FetchContactListsJsonAsync());
        }

        public async Task<dynamic> FetchContactDataByEmail(string address, APIKey apiKey)
        {
            dynamic contact = null;

            using(HttpClient client = new HttpClient())
            {
                string url = 
                    "https://api.constantcontact.com/v2/contacts?api_key=" 
                        + apiKey.ConstantContactPublicKey
                        + "&email=" + System.Web.HttpUtility.UrlEncode(address);
                
                using(HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    req.Headers.Add("Authorization", "Bearer " + apiKey.ConstantContactPrivateKey);

                    using(HttpResponseMessage response = await client.SendAsync(req))
                    {
                        dynamic result = 
                            JsonConvert.DeserializeObject<ExpandoObject>(await response.Content.ReadAsStringAsync());

                        if(result.results != null && result.results.Count > 0)
                            contact = result.results[0];
                    }
                }
            }

            return contact;
        }

        public async Task AddContact(dynamic contact, APIKey apiKey)
        {
            using(HttpClient client = new HttpClient())
            {
                string url = 
                    "https://api.constantcontact.com/v2/contacts?api_key=" 
                        + apiKey.ConstantContactPublicKey
                        + "&action_by=ACTION_BY_OWNER";
                
                using(HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    req.Headers.Add("Authorization", "Bearer " + apiKey.ConstantContactPrivateKey);
                    req.Content = new StringContent(JsonConvert.SerializeObject(contact));
                    req.Content.Headers.ContentType.MediaType = "application/json";

                    using(HttpResponseMessage response = await client.SendAsync(req))
                    {
                        if(!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(await response.Content.ReadAsStringAsync());

                            throw new Exception("Unable to add contact.");
                        }
                    }
                }
            }
        }

        public async Task UpdateContact(dynamic contact, APIKey apiKey)
        {
            using(HttpClient client = new HttpClient())
            {
                if(contact.id == null) throw new Exception("Contact object must supply ID.");

                string url = 
                    "https://api.constantcontact.com/v2/contacts/" + contact.id
                        + "?api_key=" + apiKey.ConstantContactPublicKey
                        + "&action_by=ACTION_BY_OWNER";
                
                using(HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, url))
                {
                    req.Headers.Add("Authorization", "Bearer " + apiKey.ConstantContactPrivateKey);
                    req.Content = new StringContent(JsonConvert.SerializeObject(contact));
                    req.Content.Headers.ContentType.MediaType = "application/json";

                    using(HttpResponseMessage response = await client.SendAsync(req))
                    {
                        if(!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Response status code: " + response.StatusCode);
                            Console.WriteLine("Response body: ");
                            Console.WriteLine(await response.Content.ReadAsStringAsync());
                            
                            throw new Exception("Unable to update contact.");
                        }
                    }
                }
            }
        }

        private async Task<string> FetchContactListsJsonAsync()
        {
            APIKey apiKey = await _apiKeyService.GetAPIKeyAsync();

            string result = string.Empty;

            using(HttpClient client = new HttpClient())
            {
                string url = "https://api.constantcontact.com/v2/lists?api_key=" + apiKey.ConstantContactPublicKey;
                
                using(HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    req.Headers.Add("Authorization", "Bearer " + apiKey.ConstantContactPrivateKey);

                    using(HttpResponseMessage response = await client.SendAsync(req))
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }
    }

    

    
}



