using HiddenGems.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace HiddenGems.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            HttpContext.Session.SetString("init", "");
        }
    }
}
