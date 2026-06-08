using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;

namespace nhom1dotnet.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _db;

        public LoginModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string Message { get; set; } = "";

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("username") != null)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return Page();
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.email == Username && u.password == Password);

            if (user == null)
            {
                Message = "Sai tài khoản hoặc mật khẩu.";
                return Page();
            }

            HttpContext.Session.SetString("username", user.email);
            HttpContext.Session.SetString("full_name", user.full_name);

            return RedirectToPage("/Index");
        }
    }
}
