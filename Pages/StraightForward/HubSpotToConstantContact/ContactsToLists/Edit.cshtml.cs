using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AudiocraticAPI;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;

namespace AudiocraticAPI.Pages.StraightForward.HubSpot
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;
        private readonly IConstantContactService _constantContactService;

        public EditModel(
            AudiocraticAPI.APIContext context,
            IConstantContactService constantContactService)
        {
            _context = context;
            _constantContactService = constantContactService;
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

            ViewData["ContactLists"] = new SelectList(
                await _constantContactService.FetchContactListsAsync(), "id", "name");
                return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if(id == null) return NotFound();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            ContactTypeToListRelationship.ID = (int)id;

            _context.Attach(ContactTypeToListRelationship).State = EntityState.Modified;

            

            ContactTypeToListRelationship.User = await _context.AspNetUsers.FirstAsync(
                u => u.UserName == HttpContext.User.Identity.Name
            );

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactTypeToListRelationshipExists(ContactTypeToListRelationship.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ContactTypeToListRelationshipExists(int id)
        {
            return _context.ContactTypeToListRelationship.Any(e => e.ID == id);
        }
    }
}
