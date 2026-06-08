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
        private readonly nhom1dotnet.Data.AppDbContext _context;

        public IndexModel(nhom1dotnet.Data.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public int TotalCustomers { get; set; }
        public int VipCustomers { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<CustomerViewModel> Customers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var customerQuery = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                customerQuery = customerQuery.Where(c =>
                    c.full_name.Contains(SearchString) ||
                    c.phone.Contains(SearchString) ||
                    c.email.Contains(SearchString));
            }

            var customers = await customerQuery.ToListAsync();
            var allBookings = await _context.Bookings.ToListAsync();

            Customers = customers
                .Select(customer =>
                {
                    var bookings = allBookings.Where(b => b.customer_id == customer.id).ToList();
                    var paidBookings = bookings.Where(b => b.status != "Chờ xác nhận").ToList();

                    return new CustomerViewModel
                    {
                        Id = customer.id,
                        FullName = customer.full_name,
                        Phone = customer.phone,
                        Email = customer.email,
                        CitizenId = customer.citizen_id,
                        BookingCount = bookings.Count,
                        TotalSpent = paidBookings.Sum(b => (decimal?)b.total_amount) ?? 0,
                        LastBooking = bookings.Max(b => (DateTime?)b.check_out)
                    };
                })
                .OrderByDescending(c => c.BookingCount)
                .ThenBy(c => c.FullName)
                .ToList();

            TotalCustomers = Customers.Count;
            VipCustomers = Customers.Count(c => c.BookingCount >= 5);
            TotalRevenue = Customers.Sum(c => c.TotalSpent);
        }

        public class CustomerViewModel
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string CitizenId { get; set; } = string.Empty;
            public int BookingCount { get; set; }
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