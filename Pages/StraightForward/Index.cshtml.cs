using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using AudiocraticAPI.Models;


namespace AudiocraticAPI.Pages.StraightForward
{
    [Authorize]
    public class IndexModel : PageModel
    {

        public void OnGet()
        {
        }      

        
    }
}
