using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages.Booking
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Bookings Booking { get; set; } = new Bookings();

        public List<Customer> Customers { get; set; } = new();
        public List<nhom1dotnet.Models.Room> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string? action)
        {
            await LoadSelectListsAsync();

            var customerExists = await _context.Customers
                .AnyAsync(c => c.id == Booking.customer_id);

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.id == Booking.room_id);

            if (!customerExists || room == null)
            {
                ModelState.AddModelError("", "Khách hàng hoặc phòng không tồn tại");
                return Page();
            }

            if (Booking.check_in == default || Booking.check_out == default || Booking.check_out <= Booking.check_in)
            {
                ModelState.AddModelError("", "Ngày check-out phải lớn hơn ngày check-in.");
                return Page();
            }

            var nights = (Booking.check_out - Booking.check_in).Days;
            Booking.total_amount = nights * (room.RoomType?.price ?? 0);

            if (action == "calculate")
            {
                return Page();
            }

            Booking.status = "Chờ xác nhận";
            Booking.created_at = DateTime.Now;

            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Booking/Booking");
        }

        private async Task LoadSelectListsAsync()
        {
            Customers = await _context.Customers.ToListAsync();
            Rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();
        }
    }
}