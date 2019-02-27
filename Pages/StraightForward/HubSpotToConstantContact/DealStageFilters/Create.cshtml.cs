using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using AudiocraticAPI;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;

namespace AudiocraticAPI.Pages.StraightForward.HubSpotToConstantContact.DealStageFilters
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;
        private readonly IHubSpotService _hubSpotService;
        private readonly IAPIKeyService _apiKeyService;

        public CreateModel(
            AudiocraticAPI.APIContext context,
            IHubSpotService hubSpotService,
            IAPIKeyService apiKeyService)
        {
            _context = context;
            _hubSpotService = hubSpotService;
            _apiKeyService = apiKeyService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //Get current filters (want to exclude from dropdown)
            List<DealStageFilter> filters = await _context.DealStageFilters
                .Include(d => d.User)
                .Where(d => d.User.UserName == HttpContext.User.Identity.Name)
                .ToListAsync();

            List<string> ids = filters.Select(f => f.StageID).ToList();
            
            //Fetch all deal stage options from hubspot
            APIKey key = await _apiKeyService.GetAPIKeyAsync();
            List<dynamic> dealStages = await _hubSpotService.GetDealStagesAsync(key);

            List<dynamic> options = new List<dynamic>();

            foreach(dynamic stage in dealStages)
            {
                if(!ids.Contains(stage.stageId.ToString()))
                {
                    dynamic value = new ExpandoObject();

                    value.label = stage.label;
                    value.stageId = stage.stageId;
                    value.pipelineLabel = stage.pipelineLabel;

                    dynamic option = new ExpandoObject();

                    option.DisplayText = stage.pipelineLabel + " - " + stage.label;
                    option.Value = JsonConvert.SerializeObject(value);

                    options.Add(option);
                } 
            }
            
            ViewData["UserId"] = new SelectList(options, "Value", "DisplayText");
            return Page();
        }

        [BindProperty]
        public DealStageFilter DealStageFilter { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            dynamic requestDealStageFilter = 
                JsonConvert.DeserializeObject<dynamic>(Request.Form["DealStages"]);

            DealStageFilter.StageName = 
                requestDealStageFilter.pipelineLabel + " - " + requestDealStageFilter.label;

            DealStageFilter.StageID = requestDealStageFilter.stageId;

            DealStageFilter.User = await _context.AspNetUsers
                    .FirstAsync(u => u.UserName == HttpContext.User.Identity.Name);

            _context.DealStageFilters.Add(DealStageFilter);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}