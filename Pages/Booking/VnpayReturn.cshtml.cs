using Microsoft.AspNetCore.Mvc.RazorPages;
using nhom1dotnet.Data;

namespace nhom1dotnet.Pages.Booking
{
    public class VnpayReturnModel : PageModel
    {
        private readonly AppDbContext _context;
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public VnpayReturnModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var responseCode = Request.Query["vnp_ResponseCode"];
            var txnRef = Request.Query["vnp_TxnRef"].ToString();
            var bookingIdStr = txnRef.Split('_')[0]; // ✅ thêm dòng này

            if (responseCode == "00")
            {
                IsSuccess = true;
                Message = "Thanh toán thành công!";

                if (int.TryParse(bookingIdStr, out int bookingId)) // ✅ đổi txnRef → bookingIdStr
                {
                    var booking = await _context.Bookings.FindAsync(bookingId);
                    if (booking != null)
                    {
                        booking.status = "Đã xác nhận";
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                IsSuccess = false;
                Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
            }
        }
    }
}