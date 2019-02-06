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
    public class IndexModel : PageModel
    {
        private readonly AudiocraticAPI.APIContext _context;

        public IndexModel(AudiocraticAPI.APIContext context)
        {
            _context = context;
        }

        public IList<ContactTypeToListRelationship> ContactTypeToListRelationship { get;set; }

        public async Task OnGetAsync()
        {
            ContactTypeToListRelationship = await _context.ContactTypeToListRelationship
                .Include(c => c.User)
                .Where(c => c.User.UserName == HttpContext.User.Identity.Name)
                .ToListAsync();
        }
    }
}
