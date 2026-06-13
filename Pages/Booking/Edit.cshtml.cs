using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages.Booking
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Bookings Booking { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Booking = await _context.Bookings.FindAsync(id);

            if (Booking == null)
            {
                return RedirectToPage("/Booking/Booking");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var data = await _context.Bookings.FindAsync(Booking.id);
            if (data == null)
            {
                return RedirectToPage("/Booking/Booking");
            }

            data.customer_id = Booking.customer_id;
            data.room_id = Booking.room_id;
            data.check_in = Booking.check_in;
            data.check_out = Booking.check_out;
            data.status = Booking.status;

            //tính lại giá tiền khi thay đổi số đêm
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.id == Booking.room_id);

            if (room != null)
            {
                var nights = (Booking.check_out - Booking.check_in).Days;
                data.total_amount = nights * room.RoomType.price;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Booking/Booking");
        }
    }
}