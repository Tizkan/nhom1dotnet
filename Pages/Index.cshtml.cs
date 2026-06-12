using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalBookings { get; set; }

        public IActionResult OnGet()
        {
            var user = HttpContext.Session.GetString("username");

            if (user == null)
            {
                return RedirectToPage("/Login/Login");
            }

            TotalBookings = _context.Bookings.Count();

            return Page();
        }
    }

    internal class ApplicationDbContext
    {
        public object Bookings { get; internal set; }
    }
}