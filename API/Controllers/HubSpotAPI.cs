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
using AudiocraticAPI.Services;

[Produces("application/json")]
[Route("straightforward/[controller]")]
[ApiController]
public class HubSpotController : ControllerBase
{
    public dynamic APIKeys { get; set; }

    private readonly AudiocraticAPI.APIContext _context;
    private readonly IAPIKeyService _apiKeyService;
    private readonly IHubSpotService _hubSpotService;
    private readonly IHubSpotToConstantContactService _hubSpotToConstantContactService;

    public HubSpotController(
        AudiocraticAPI.APIContext context,
        IAPIKeyService apiKeyService,
        IHubSpotService hubSpotService,
        IHubSpotToConstantContactService hubSpotToConstantContactService)
    {
        _context = context;
        _apiKeyService = apiKeyService;
        _hubSpotService = hubSpotService;
        _hubSpotToConstantContactService = hubSpotToConstantContactService;
    }
    
    
    [HttpPost]
    public async Task<IActionResult> ExecuteAsync(string apiKey)
    {
        //Validate API Key
        APIKey currentAPIKey = await _apiKeyService.GetAPIKeyByKeyAsync(apiKey);

        if(currentAPIKey == null) return Unauthorized();

        IActionResult result = null;

        // try
        // {
                result = 
                await _hubSpotToConstantContactService.TransferContactsFromHubSpotToConstantContact(
                        GetDataFromRequest(),
                        currentAPIKey
                    );
        // }
        // catch(Exception ex)
        // {
        //     GetErrorJson(ex.Message);
        // }

        return result;
    }

    private JsonResult GetErrorJson(string message)
    {
        dynamic error = new ExpandoObject();

        error.Description = message;

        return new JsonResult(error);
    }

    private dynamic GetDataFromRequest()
    {
        using(StreamReader reader = new StreamReader(Request.Body))
        {
            string data = reader.ReadToEnd();
            
            Console.WriteLine("Event data Json payload received: " + data); //Log

            return data;
        }
    }
    
    
}