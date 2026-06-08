using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace nhom1dotnet.Pages
{
    public class SettingsModel : PageModel
    {
        public IActionResult OnGet()
        {
            var user = HttpContext.Session.GetString("username");

            if (user == null)
            {
                return RedirectToPage("/Login/Login");
            }

            return Page();
        }
    }
}