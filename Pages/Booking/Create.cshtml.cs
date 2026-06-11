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

        public CreateModel(AppDbContext context)//tự động tạo và truyền vào context để dùng xuyên suốt
        {
            _context = context;
        }
        
        //tự động map dữ liệu lên khi form post (id)
        [BindProperty]
        public Bookings Booking { get; set; } = new Bookings();

        public List<Customer> Customers { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();

        //load ds Cus và Room
        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        //khi nhấm lưu
        public async Task<IActionResult> OnPostAsync()
        {   
            //Mỗi khi POST lên, bộ nhớ reset sạch 
            // — Customers và Rooms trở về rỗng. 
            // Load lại ngay đầu để phòng trường hợp dữ liệu lỗi phải return Page() 
            // — lúc đó form cần có data để render lại dropdown, nếu không dropdown sẽ trống.
            await LoadSelectListsAsync();

            //AnyAsync chỉ kiểm tra có tồn tại không, trả về true/false — nhanh hơn load cả object.
            var customerExists = await _context.Customers
                .AnyAsync(c => c.id == Booking.customer_id);

            var room = await _context.Rooms
                .Include(r => r.RoomType)//lấy data RoomType để tính tiền 
                .FirstOrDefaultAsync(r => r.id == Booking.room_id);

            if (!customerExists || room == null)
            {
                ModelState.AddModelError("", "Khách hàng hoặc phòng không tồn tại");
                return Page();
            }

            //mặc định 01/01/0001 (user chưa chọn) và check ngày vào ngày ra tránh ngày âm
            if (Booking.check_in == default || Booking.check_out == default || Booking.check_out <= Booking.check_in)
            {
                ModelState.AddModelError("", "Ngày check-out phải lớn hơn ngày check-in.");
                return Page();
            }

            //tính tiền luôn trước khi lưu
            var nights = (Booking.check_out - Booking.check_in).Days;
            Booking.total_amount = nights * (room.RoomType?.price ?? 0);//? tránh crash nếu null -null thì trả về ?? 0 

            //mặc định chờ xác nhận sau khi tạo
            Booking.status = "Chờ xác nhận";
            Booking.created_at = DateTime.Now;//tg hiện tại

            _context.Bookings.Add(Booking);//lưu vào context 
            await _context.SaveChangesAsync();//insert lưu vào db 

            return RedirectToPage("/Booking/Booking");
        }

        //hàm để load ds thay vì phải viết từng hàm
        private async Task LoadSelectListsAsync()
        {
            Customers = await _context.Customers.ToListAsync();
            Rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();
        }
    }
}