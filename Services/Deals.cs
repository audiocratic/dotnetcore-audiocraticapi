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
    public interface IDealService
    {
        Deal GetDealFromHubSpotDealData(dynamic data);
        Deal GetDealFromHubSpotDealData(dynamic data, APIKey aPIKey);
    }

    public class DealService : IDealService
    {
        public Deal GetDealFromHubSpotDealData(dynamic data)
        {
            return GetDealFromHubSpotDealData(data, null);
        }

        public Deal GetDealFromHubSpotDealData(dynamic data, APIKey apiKey)
        {
            Deal deal = new Deal();

            //Deal ID is required on the object from HubSpot
            if(!((ExpandoObject)data).HasProperty("dealId")) 
                throw new Exception("Invalid HubSpot deal data.");

            deal.ID = (int)data.dealId;

            if(((ExpandoObject)data).HasProperty("properties.dealname"))
                deal.Name = data.properties.dealname.value;

            if(((ExpandoObject)data).HasProperty("properties.dealstage"))
                deal.StageID = data.properties.dealstage.value;

            if(apiKey != null) deal.UserId = apiKey.UserId;

            return deal;
        }
    }

}