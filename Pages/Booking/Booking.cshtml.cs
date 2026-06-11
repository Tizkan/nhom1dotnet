using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;
using Microsoft.AspNetCore.Mvc;

namespace nhom1dotnet.Pages.Booking
{
    public class BookingModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly VnPayService _vnPay;

        public BookingModel(AppDbContext context, VnPayService vnPay)
        {
            _context = context;
            _vnPay = vnPay;
        }

        public IList<Bookings> Bookings { get; set; }

        public async Task OnGetAsync()
        {
            
            Bookings = await _context.Bookings
                .Include(b => b.Customer) 
                .Include(b => b.Room)   
                .ToListAsync();

        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostPay(int id)
        {   
            var booking = _context.Bookings.FirstOrDefault(x => x.id == id);

            if (booking == null)
                return RedirectToPage();

            Console.WriteLine("AMOUNT: " + booking.total_amount);

            var url = _vnPay.CreatePaymentUrl(
                HttpContext,
                booking.total_amount ?? 0,
                booking.id
            );

            Console.WriteLine("VNPAY URL: " + url);
            return Redirect(url);
        }
    }
}