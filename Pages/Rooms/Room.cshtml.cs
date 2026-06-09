using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages.Rooms
{
    public class RoomModel : PageModel
    {
        private readonly AppDbContext _db;
        public RoomModel(AppDbContext db) => _db = db;

        // ── Query params ──────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FloorFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        // ── Data ─────────────────────────────────────────────
        public List<nhom1dotnet.Models.Room>     Rooms     { get; set; } = new();
        public List<RoomType> RoomTypes { get; set; } = new();
        public List<int>      Floors    { get; set; } = new();

        // ── Messages ──────────────────────────────────────────
        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage   { get; set; } = "";

        // ─────────────────────────────────────────────────────
        public async Task OnGetAsync(string? msg, string? err)
        {
            SuccessMessage = msg ?? "";
            ErrorMessage   = err ?? "";

            RoomTypes = await _db.RoomTypes.OrderBy(t => t.name).ToListAsync();

            // Distinct floors từ DB
            Floors = await _db.Rooms
                .Select(r => r.floor_number)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            var query = _db.Rooms.Include(r => r.RoomType).AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(r =>
                    r.room_number.Contains(Search) ||
                    (r.RoomType != null && r.RoomType.name.Contains(Search)));

            if (FloorFilter.HasValue)
                query = query.Where(r => r.floor_number == FloorFilter.Value);

            if (!string.IsNullOrWhiteSpace(StatusFilter))
                query = query.Where(r => r.status == StatusFilter);

            Rooms = await query.OrderBy(r => r.floor_number).ThenBy(r => r.room_number).ToListAsync();
        }

        // ── THÊM ─────────────────────────────────────────────
        public async Task<IActionResult> OnPostAddAsync(
            string roomNumber, int roomTypeId, int floorNumber, string status)
        {
            if (string.IsNullOrWhiteSpace(roomNumber))
                return RedirectToPage(new { err = "Vui lòng nhập số phòng." });

            if (await _db.Rooms.AnyAsync(r => r.room_number == roomNumber.Trim()))
                return RedirectToPage(new { err = $"Số phòng \"{roomNumber.Trim()}\" đã tồn tại." });

            if (!await _db.RoomTypes.AnyAsync(t => t.id == roomTypeId))
                return RedirectToPage(new { err = "Loại phòng không hợp lệ." });

            if (floorNumber < 1 || floorNumber > 99)
                return RedirectToPage(new { err = "Số tầng phải từ 1 đến 99." });

            _db.Rooms.Add(new nhom1dotnet.Models.Room
            {
                room_number  = roomNumber.Trim(),
                room_type_id = roomTypeId,
                floor_number = floorNumber,
                status       = status
            });

            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã thêm phòng {roomNumber.Trim()} thành công." });
        }

        // ── SỬA ──────────────────────────────────────────────
        public async Task<IActionResult> OnPostEditAsync(
            int id, string roomNumber, int roomTypeId, int floorNumber, string status)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return RedirectToPage(new { err = "Không tìm thấy phòng." });

            if (string.IsNullOrWhiteSpace(roomNumber))
                return RedirectToPage(new { err = "Vui lòng nhập số phòng." });

            if (await _db.Rooms.AnyAsync(r => r.room_number == roomNumber.Trim() && r.id != id))
                return RedirectToPage(new { err = $"Số phòng \"{roomNumber.Trim()}\" đã được dùng bởi phòng khác." });

            if (floorNumber < 1 || floorNumber > 99)
                return RedirectToPage(new { err = "Số tầng phải từ 1 đến 99." });

            room.room_number  = roomNumber.Trim();
            room.room_type_id = roomTypeId;
            room.floor_number = floorNumber;
            room.status       = status;

            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã cập nhật phòng {roomNumber.Trim()} thành công." });
        }

        // ── XÓA ──────────────────────────────────────────────
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return RedirectToPage(new { err = "Không tìm thấy phòng." });

            // Kiểm tra có booking đang dùng không
            bool hasBooking = await _db.Bookings.AnyAsync(b => b.room_id == id);
            if (hasBooking)
                return RedirectToPage(new { err = $"Không thể xóa phòng {room.room_number} vì đang có đặt phòng liên kết." });

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã xóa phòng {room.room_number} thành công." });
        }
    }
}
