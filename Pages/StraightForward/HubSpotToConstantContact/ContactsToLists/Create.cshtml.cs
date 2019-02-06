using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AudiocraticAPI;
using AudiocraticAPI.Models;
using AudiocraticAPI.Services;

namespace AudiocraticAPI.Pages.StraightForward.HubSpot
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;
        private readonly IConstantContactService _constantContactService;
        
        [BindProperty]
        public ContactTypeToListRelationship ContactTypeToListRelationship { get; set; }
        

        public CreateModel(
            AudiocraticAPI.APIContext context,
            IConstantContactService constantContactService)
        {
            _context = context;
            _constantContactService = constantContactService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ContactLists"] = new SelectList(
                await _constantContactService.FetchContactListsAsync(), "id", "name");
                return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("Create");
            }

            ContactTypeToListRelationship.User = await _context.AspNetUsers.FirstAsync(
                u => u.UserName == HttpContext.User.Identity.Name
            );

            _context.ContactTypeToListRelationship.Add(ContactTypeToListRelationship);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}