using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages_Staff
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public int TotalStaff { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public List<StaffViewModel> Staffs { get; set; } = new();

        public async Task OnGetAsync(string? msg, string? err)
        {
            SuccessMessage = msg ?? string.Empty;
            ErrorMessage   = err ?? string.Empty;

            var query = _context.Staffs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                query = query.Where(s =>
                    s.full_name.Contains(SearchString) ||
                    (s.phone != null && s.phone.Contains(SearchString)) ||
                    s.email.Contains(SearchString));
            }

            var staffList = await query.OrderBy(s => s.full_name).ToListAsync();

            Staffs = staffList.Select(s => new StaffViewModel
            {
                Id         = s.id,
                FullName   = s.full_name,
                Email      = s.email,
                Phone      = s.phone ?? string.Empty,
                CitizenId  = s.citizen_id ?? string.Empty,
                BirthDate  = s.birth_date,
                CreatedAt  = s.created_at
            }).ToList();

            TotalStaff = Staffs.Count;
        }

        public async Task<IActionResult> OnPostAddAsync(
            string fullname, string email, string? phone, string? citizenid, string? birthdate)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email))
                return RedirectToPage(new { err = "Vui lòng điền đầy đủ họ tên và email." });

            DateOnly? birth = null;
            if (!string.IsNullOrWhiteSpace(birthdate) && DateOnly.TryParse(birthdate, out var parsed))
                birth = parsed;

            _context.Staffs.Add(new Staff
            {
                full_name  = fullname.Trim(),
                email      = email.Trim(),
                phone      = phone?.Trim(),
                citizen_id = citizenid?.Trim(),
                birth_date = birth,
                created_at = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã thêm nhân viên \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostEditAsync(
            int id, string fullname, string email, string? phone, string? citizenid, string? birthdate)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email))
                return RedirectToPage(new { err = "Vui lòng điền đầy đủ họ tên và email." });

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return RedirectToPage(new { err = "Không tìm thấy nhân viên." });

            DateOnly? birth = null;
            if (!string.IsNullOrWhiteSpace(birthdate) && DateOnly.TryParse(birthdate, out var parsed))
                birth = parsed;

            staff.full_name  = fullname.Trim();
            staff.email      = email.Trim();
            staff.phone      = phone?.Trim();
            staff.citizen_id = citizenid?.Trim();
            staff.birth_date = birth;

            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã cập nhật nhân viên \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
                return RedirectToPage(new { err = "Không tìm thấy nhân viên." });

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã xóa nhân viên \"{staff.full_name}\" thành công." });
        }

        public class StaffViewModel
        {
            public int      Id        { get; set; }
            public string   FullName  { get; set; } = string.Empty;
            public string   Email     { get; set; } = string.Empty;
            public string   Phone     { get; set; } = string.Empty;
            public string   CitizenId { get; set; } = string.Empty;
            public DateOnly? BirthDate { get; set; }
            public DateTime CreatedAt { get; set; }

            public string Initials => string.IsNullOrWhiteSpace(FullName)
                ? "?"
                : string.Join(string.Empty,
                    FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x[0])
                        .Take(2))
                    .ToUpperInvariant();
        }
    }
}