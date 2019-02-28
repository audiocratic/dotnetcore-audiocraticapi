using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AudiocraticAPI;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;

namespace AudiocraticAPI.Pages.StraightForward.HubSpotToConstantContact.Logs
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHubSpotToConstantContactService _hubSpotToConstantContactService;
        private readonly HubSpotToCCIntegrationLogService  _logService;
        private readonly APIContext _context;
        private readonly IAPIKeyService _apiKeyService;

        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { 
            get 
            {
                return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
            }
        }

        public List<DealStageChange> DealStageChanges { get; set; }

        public IndexModel(
            IHubSpotToConstantContactService hubSpotToConstantContactService,
            HubSpotToCCIntegrationLogService logService,
            APIContext context,
            IAPIKeyService aPIKeyService)
        {
            _hubSpotToConstantContactService = hubSpotToConstantContactService;
            _logService = logService;
            _context = context;
            _apiKeyService = aPIKeyService;
        }

        [HttpPost]
        public async Task<IActionResult> OnPostReSyncAsync(int? dealId)
        {
            if(dealId == null) return NotFound();

            APIKey key = await _apiKeyService.GetAPIKeyAsync();
            
            await _hubSpotToConstantContactService
                .TransferContactsFromHubSpotToConstantContact((int)dealId, key);

            return RedirectToPage("Index");
        }

        public async Task OnGetAsync(int? pageNumber)
        {
            PageNumber = pageNumber == null ? 1 : (int)pageNumber;
            ItemsPerPage = 10;
            int skip = (PageNumber - 1) * ItemsPerPage;

            TotalItems = await _logService.GetTotalDealStageChanges(HttpContext.User.Identity.Name);

            DealStageChanges = 
                await _logService.GetDealStageChangesAsync(HttpContext.User.Identity.Name, skip, ItemsPerPage);
        }
    }
}