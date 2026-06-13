using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nhom1dotnet.Data;
using nhom1dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace nhom1dotnet.Pages
{
    public class IndexModel : PageModel 
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Thống kê
        public int TotalBookings { get; set; }
        public int TotalCustomers { get; private set; }
        public decimal TotalRevenue { get; set; }
        
        //Hoạt động gần đây
        public List<ActivityItem> RecentActivities { get; set; } = new();

        //Danh sách phòng trống
        public List<Room> AvailableRooms { get; set; } = new();

        public IActionResult OnGet()
        {
            var user = HttpContext.Session.GetString("username");

            if (user == null)
            {
                return RedirectToPage("/Login/Login");
            }

            // Thống kê tổng quan
            TotalBookings = _context.Bookings.Count();
            TotalCustomers = _context.Customers.Count();
            TotalRevenue = _context.Bookings
                            .Where(b => b.status != "Chờ xác nhận")
                            .Sum(b => (decimal?)b.total_amount) ?? 0;
    

            // Hoạt động gần đây (10 booking mới nhất)
            RecentActivities = _context.Bookings
                .OrderByDescending(b => b.created_at)
                .Take(10)
                .Select(b => new ActivityItem
                {
                    Time = b.created_at.ToString("HH:mm"),

                    Title = $"Đặt phòng #{b.id}",

                    CustomerName = _context.Customers
                        .Where(c => c.id == b.customer_id)
                        .Select(c => c.full_name)
                        .FirstOrDefault() ?? "Không xác định"
                })
                .ToList();

            // Danh sách phòng trống
            AvailableRooms = _context.Rooms
                .Where(r => r.status == "Available")
                .Include(r => r.RoomType)
                .Take(10)
                .ToList();

            return Page();
            }

    public class ActivityItem
    {
        public string Time { get; set; } = "";
        public string Title { get; set; } = "";
        public string CustomerName { get; set; } = "";
    }

    public class RoomWithTypeName
    {
        public int room_number { get; set; }
        public string type_name { get; set; } = "";
    }
    }
 }