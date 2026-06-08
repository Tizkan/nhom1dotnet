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
        public List<Room> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            Customers = await _context.Customers.ToListAsync();
            Rooms = await _context.Rooms.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var customerExists = await _context.Customers
                .AnyAsync(c => c.id == Booking.customer_id);

            var roomExists = await _context.Rooms
                .AnyAsync(r => r.id == Booking.room_id);

            if (!customerExists || !roomExists)
            {
                ModelState.AddModelError("", "Khách hàng hoặc phòng không tồn tại");
                return Page();
            }

            Booking.status = "Chờ xác nhận";
            Booking.created_at = DateTime.Now;

            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Booking/Booking");
        }
    }
}