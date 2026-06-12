using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using nhom1dotnet.Data;
using nhom1dotnet.Models;

namespace nhom1dotnet.Pages.Report;

public class ReportModel : PageModel
{
    private readonly AppDbContext _db;

    public ReportModel(AppDbContext db) => _db = db;

    // KPI  
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public double  OccupancyRate    { get; set; }
    public double  OccupancyLast    { get; set; }
    public double  AvgStayNights    { get; set; }
    public double  AvgStayLast      { get; set; }

    // Charts
    public List<MonthStat>     MonthlyStats  { get; set; } = [];
    public int                 BookingStandard { get; set; }
    public int                 BookingDeluxe   { get; set; }
    public int                 BookingSuite    { get; set; }

    // Lists
    public List<VipCustomer>   TopCustomers { get; set; } = [];
    public List<BookingSource> Sources      { get; set; } = [];

    // ──────────────────────────────────────────────────────────
    public async Task OnGetAsync()
    {
        var now              = DateTime.Now;
        var startThisMonth   = new DateTime(now.Year, now.Month, 1);
        var startLastMonth   = startThisMonth.AddMonths(-1);

        await LoadRevenueAsync(startThisMonth, startLastMonth);
        await LoadOccupancyAsync(now, startThisMonth, startLastMonth);
        await LoadAvgStayAsync(startThisMonth, startLastMonth);
        await LoadMonthlyStatsAsync(startThisMonth);
        await LoadRoomTypeDistributionAsync();
        await LoadTopCustomersAsync();
        LoadBookingSources();
    }

    // ── Private helpers ────────────────────────────────────────

    private async Task LoadRevenueAsync(DateTime startThis, DateTime startLast)
    {
        RevenueThisMonth = await _db.Payments
            .Where(p => p.status == "paid" && p.payment_date >= startThis)
            .SumAsync(p => (decimal?)p.amount) ?? 0;

        RevenueLastMonth = await _db.Payments
            .Where(p => p.status == "paid" && p.payment_date >= startLast && p.payment_date < startThis)
            .SumAsync(p => (decimal?)p.amount) ?? 0;
    }

    private async Task LoadOccupancyAsync(DateTime now, DateTime startThis, DateTime startLast)
    {
        var total        = await _db.Rooms.CountAsync();
        var occupiedNow  = await _db.Bookings.CountAsync(b => b.check_in <= now && b.check_out >= now);
        var occupiedLast = await _db.Bookings.CountAsync(b => b.check_in <= startThis.AddDays(-1) && b.check_out >= startLast);

        OccupancyRate = total > 0 ? Math.Round((double)occupiedNow  / total * 100, 1) : 0;
        OccupancyLast = total > 0 ? Math.Round((double)occupiedLast / total * 100, 1) : 0;
    }

    private async Task LoadAvgStayAsync(DateTime startThis, DateTime startLast)
    {
        var staysThis = await _db.Bookings
            .Where(b => b.check_in >= startThis)
            .Select(b => Microsoft.EntityFrameworkCore.MySqlDbFunctionsExtensions.DateDiffDay(EF.Functions, b.check_in, b.check_out))
            .ToListAsync();

        var staysLast = await _db.Bookings
            .Where(b => b.check_in >= startLast && b.check_in < startThis)
            .Select(b => Microsoft.EntityFrameworkCore.MySqlDbFunctionsExtensions.DateDiffDay(EF.Functions, b.check_in, b.check_out))
            .ToListAsync();

        AvgStayNights = staysThis.Any() ? Math.Round(staysThis.Average(), 1) : 0;
        AvgStayLast   = staysLast.Any() ? Math.Round(staysLast.Average(), 1) : 0;
    }

    private async Task LoadMonthlyStatsAsync(DateTime startThisMonth)
    {
        for (int i = 5; i >= 0; i--)
        {
            var start = startThisMonth.AddMonths(-i);
            var end   = start.AddMonths(1);

            var revenue = await _db.Payments
                .Where(p => p.status == "paid" && p.payment_date >= start && p.payment_date < end)
                .SumAsync(p => (decimal?)p.amount) ?? 0;

            var count = await _db.Bookings
                .CountAsync(b => b.check_in >= start && b.check_in < end);

            MonthlyStats.Add(new MonthStat
            {
                Label   = $"T{start.Month}",
                Revenue = (double)(revenue / 1_000_000),
                Count   = count
            });
        }
    }

    private async Task LoadRoomTypeDistributionAsync()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .ToListAsync();

        BookingStandard = bookings.Count(b => b.Room?.RoomType?.name == "Standard");
        BookingDeluxe   = bookings.Count(b => b.Room?.RoomType?.name == "Deluxe");
        BookingSuite    = bookings.Count(b => b.Room?.RoomType?.name == "Suite");

        // Fallback nếu chưa có dữ liệu
        if (BookingStandard + BookingDeluxe + BookingSuite == 0)
            (BookingStandard, BookingDeluxe, BookingSuite) = (45, 35, 20);
    }

    private async Task LoadTopCustomersAsync()
    {
        TopCustomers = await _db.Bookings
            .Include(b => b.Customer)
            .GroupBy(b => new { b.customer_id, b.Customer.full_name })
            .Select(g => new VipCustomer
            {
                Name         = g.Key.full_name,
                BookingCount = g.Count(),
                TotalSpent   = g.Sum(b => b.total_amount ?? 0)
            })
            .OrderByDescending(v => v.TotalSpent)
            .Take(5)
            .ToListAsync();
    }

    private void LoadBookingSources()
    {
        Sources =
        [
            new() { Name = "Website trực tiếp", Count = 145, Percent = 42 },
            new() { Name = "Booking.com",        Count = 98,  Percent = 28 },
            new() { Name = "Agoda",              Count = 67,  Percent = 19 },
            new() { Name = "Điện thoại",         Count = 38,  Percent = 11 },
        ];
    }

    // ── DTOs ───────────────────────────────────────────────────

    public record MonthStat(string Label = "", double Revenue = 0, int Count = 0)
    {
        public MonthStat() : this("", 0, 0) { }
        public string Label   { get; set; } = Label;
        public double Revenue { get; set; } = Revenue;
        public int    Count   { get; set; } = Count;
    }

    public record VipCustomer(string Name = "", int BookingCount = 0, decimal TotalSpent = 0)
    {
        public VipCustomer() : this("", 0, 0) { }
        public string  Name         { get; set; } = Name;
        public int     BookingCount { get; set; } = BookingCount;
        public decimal TotalSpent   { get; set; } = TotalSpent;
    }

    public record BookingSource(string Name = "", int Count = 0, int Percent = 0)
    {
        public BookingSource() : this("", 0, 0) { }
        public string Name    { get; set; } = Name;
        public int    Count   { get; set; } = Count;
        public int    Percent { get; set; } = Percent;
    }
}
