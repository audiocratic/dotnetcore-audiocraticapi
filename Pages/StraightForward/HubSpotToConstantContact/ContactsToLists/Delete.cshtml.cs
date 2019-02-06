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

namespace AudiocraticAPI.Pages.StraightForward.HubSpot
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
        public ContactTypeToListRelationship ContactTypeToListRelationship { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContactTypeToListRelationship = await _context.ContactTypeToListRelationship
                .Include(c => c.User)
                .Where(c => c.User.UserName == HttpContext.User.Identity.Name)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (ContactTypeToListRelationship == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContactTypeToListRelationship = 
                await _context.ContactTypeToListRelationship
                .Where(c => c.User.UserName == HttpContext.User.Identity.Name)
                .FirstOrDefaultAsync(c => c.ID == (int)id);

            if (ContactTypeToListRelationship != null)
            {
                _context.ContactTypeToListRelationship.Remove(ContactTypeToListRelationship);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
