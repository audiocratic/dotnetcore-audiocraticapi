using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;

namespace AudiocraticAPI.Pages.StraightForward
{
    
    [Authorize]
    public class ManageKeyModel : PageModel
    {
        [BindProperty]
        public APIKey APIKey { get; set; }

        private readonly AudiocraticAPI.APIContext _context;
        private readonly IAPIKeyService _apiKeyService;

        public ManageKeyModel (
            AudiocraticAPI.APIContext context,
            IAPIKeyService apiKeyService) 
        {
            _context = context;
            _apiKeyService = apiKeyService;
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUpdateExternalKeysAsync()
        {
            await _apiKeyService.UpdateAPIKeyAsync(APIKey);
            
            return RedirectToPage("ManageKey");
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync()
        {
            await _apiKeyService.CreateNewAPIKeyAsync();
            
            return RedirectToPage("ManageKey");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            APIKey = await _apiKeyService.GetAPIKeyAsync();

            return Page();
        }

        
    }
}