using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages_Staff
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        public IndexModel(AppDbContext db) => _db = db;

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = "";

        public List<Staff> Staffs { get; set; } = new();
        public int TotalStaff { get; set; }
        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public async Task OnGetAsync(string? msg, string? err)
        {
            SuccessMessage = msg ?? "";
            ErrorMessage = err ?? "";

            var query = _db.Staffs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
                query = query.Where(s =>
                    s.full_name.Contains(SearchString) ||
                    s.email.Contains(SearchString) ||
                    (s.citizen_id != null && s.citizen_id.Contains(SearchString)));

            Staffs = await query.OrderBy(s => s.full_name).ToListAsync();
            TotalStaff = Staffs.Count;
        }

        public async Task<IActionResult> OnPostAddAsync(
            string fullname, string email, string? birthdate, string? citizenid, string? phone)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email))
                return RedirectToPage(new { err = "Vui lòng điền đầy đủ họ tên và email." });

            if (await _db.Staffs.AnyAsync(s => s.email == email.Trim()))
                return RedirectToPage(new { err = "Email đã tồn tại trong hệ thống." });

            DateOnly? dob = null;
            if (!string.IsNullOrWhiteSpace(birthdate) &&
                DateOnly.TryParse(birthdate, out var parsed))
                dob = parsed;

            _db.Staffs.Add(new Staff
            {
                full_name = fullname.Trim(),
                email = email.Trim(),
                birth_date = dob,
                citizen_id = string.IsNullOrWhiteSpace(citizenid) ? null : citizenid.Trim(),
                phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                created_at = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã thêm nhân viên \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostEditAsync(
            int id, string fullname, string email, string? birthdate, string? citizenid, string? phone)
        {
            var staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
                return RedirectToPage(new { err = "Không tìm thấy nhân viên." });

            if (await _db.Staffs.AnyAsync(s => s.email == email.Trim() && s.id != id))
                return RedirectToPage(new { err = "Email đã được dùng bởi nhân viên khác." });

            DateOnly? dob = null;
            if (!string.IsNullOrWhiteSpace(birthdate) &&
                DateOnly.TryParse(birthdate, out var parsed))
                dob = parsed;

            staff.full_name = fullname.Trim();
            staff.email = email.Trim();
            staff.birth_date = dob;
            staff.citizen_id = string.IsNullOrWhiteSpace(citizenid) ? null : citizenid.Trim();
            staff.phone      = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();


            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã cập nhật nhân viên \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
                return RedirectToPage(new { err = "Không tìm thấy nhân viên." });

            _db.Staffs.Remove(staff);
            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã xóa nhân viên \"{staff.full_name}\" thành công." });
        }
    }
}
