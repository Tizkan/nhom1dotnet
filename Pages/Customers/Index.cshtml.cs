using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages_Customers
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

        public int TotalCustomers { get; set; }
        public int VipCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public List<CustomerViewModel> Customers { get; set; } = new();

        public async Task OnGetAsync(string? msg, string? err)
        {
            SuccessMessage = msg ?? string.Empty;
            ErrorMessage   = err ?? string.Empty;

            var customerQuery = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                customerQuery = customerQuery.Where(c =>
                    c.full_name.Contains(SearchString) ||
                    c.phone.Contains(SearchString) ||
                    c.email.Contains(SearchString));
            }

            var customers   = await customerQuery.ToListAsync();
            var allBookings = await _context.Bookings.ToListAsync();

            Customers = customers
                .Select(customer =>
                {
                    var bookings     = allBookings.Where(b => b.customer_id == customer.id).ToList();
                    var paidBookings = bookings.Where(b => b.status != "Chờ xác nhận").ToList();

                    return new CustomerViewModel
                    {
                        Id           = customer.id,
                        FullName     = customer.full_name,
                        Phone        = customer.phone,
                        Email        = customer.email,
                        Address      = customer.address,
                        CitizenId    = customer.citizen_id,
                        BookingCount = bookings.Count,
                        TotalSpent   = paidBookings.Sum(b => (decimal?)b.total_amount) ?? 0,
                        LastBooking  = bookings.Max(b => (DateTime?)b.check_out)
                    };
                })
                .OrderByDescending(c => c.BookingCount)
                .ThenBy(c => c.FullName)
                .ToList();

            TotalCustomers = Customers.Count;
            VipCustomers   = Customers.Count(c => c.BookingCount >= 5);
            TotalRevenue   = Customers.Sum(c => c.TotalSpent);
        }

        public async Task<IActionResult> OnPostAddAsync(
            string fullname, string phone, string? email, string? address, string? citizenid)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(phone))
                return RedirectToPage(new { err = "Vui lòng điền đầy đủ họ tên và số điện thoại." });

            _context.Customers.Add(new Customer
            {
                full_name  = fullname.Trim(),
                phone      = phone.Trim(),
                email      = email?.Trim() ?? string.Empty,
                address    = address?.Trim() ?? string.Empty,
                citizen_id = citizenid?.Trim() ?? string.Empty,
                created_at = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã thêm khách hàng \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostEditAsync(
            int id, string fullname, string phone, string? email, string? address, string? citizenid)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(phone))
                return RedirectToPage(new { err = "Vui lòng điền đầy đủ họ tên và số điện thoại." });

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return RedirectToPage(new { err = "Không tìm thấy khách hàng." });

            customer.full_name  = fullname.Trim();
            customer.phone      = phone.Trim();
            customer.email      = email?.Trim() ?? string.Empty;
            customer.address    = address?.Trim() ?? string.Empty;
            customer.citizen_id = citizenid?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã cập nhật khách hàng \"{fullname}\" thành công." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return RedirectToPage(new { err = "Không tìm thấy khách hàng." });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { msg = $"Đã xóa khách hàng \"{customer.full_name}\" thành công." });
        }

        public class CustomerViewModel
        {
            public int Id { get; set; }
            public string FullName     { get; set; } = string.Empty;
            public string Phone        { get; set; } = string.Empty;
            public string Email        { get; set; } = string.Empty;
            public string Address      { get; set; } = string.Empty;
            public string CitizenId    { get; set; } = string.Empty;
            public int    BookingCount { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime? LastBooking { get; set; }

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
