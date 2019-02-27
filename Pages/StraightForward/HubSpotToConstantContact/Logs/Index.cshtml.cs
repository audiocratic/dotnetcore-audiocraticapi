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
        private readonly HubSpotToCCIntegrationLogService  _logService;
        private readonly APIContext _context;

        public List<DealStageChange> DealStageChanges { get; set; }

        public IndexModel(
            HubSpotToCCIntegrationLogService logService,
            APIContext context)
        {
            _logService = logService;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            DealStageChanges = 
                await _logService.GetDealStageChangesAsync(HttpContext.User.Identity.Name);
        }
    }
}