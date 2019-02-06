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

namespace AudiocraticAPI.Pages.StraightForward.HubSpotToConstantContact.DealStageFilters
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;

        public IndexModel(AudiocraticAPI.APIContext context)
        {
            _context = context;
        }

        public IList<DealStageFilter> DealStageFilter { get;set; }

        public async Task OnGetAsync()
        {
            DealStageFilter = await _context.DealStageFilters
                .Include(d => d.User)
                .Where(d => d.User.UserName == HttpContext.User.Identity.Name)
                .ToListAsync();
        }
    }
}
