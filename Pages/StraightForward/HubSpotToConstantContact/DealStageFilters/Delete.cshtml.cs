using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AudiocraticAPI;
using AudiocraticAPI.Models;

namespace AudiocraticAPI.Pages.StraightForward.HubSpotToConstantContact.DealStageFilters
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;

        public DeleteModel(AudiocraticAPI.APIContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DealStageFilter DealStageFilter { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DealStageFilter = await _context.DealStageFilters
                .Include(d => d.User)
                .Where(d => d.User.UserName == HttpContext.User.Identity.Name)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (DealStageFilter == null)
            {
                return NotFound();
            }

            _context.DealStageFilters.Remove(DealStageFilter);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

    }
}
