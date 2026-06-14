using System.Security.Cryptography;
using System.Text;

public class VnPayService
{
    private readonly IConfiguration _config;

    public VnPayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(HttpContext context, decimal amount, int bookingId)
    {
        var tmnCode = _config["Vnpay:TmnCode"];
        var hashSecret = _config["Vnpay:HashSecret"];
        var baseUrl = _config["Vnpay:BaseUrl"];
        var returnUrl = _config["Vnpay:ReturnUrl"];
        var ip = context.Connection.RemoteIpAddress?.ToString(); 
            ip = "127.0.0.1";

        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", ((long)amount * 100).ToString() },
            { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanhtoanbooking{bookingId}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_TxnRef", $"{bookingId}_{DateTime.Now.Ticks}" },
        };
    
        var query = string.Join("&", vnpParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var hashData = query;

        var secureHash = HmacSHA512(hashSecret, hashData); 

        return baseUrl + "?" + query + "&vnp_SecureHash=" + secureHash;
    }

    private string HmacSHA512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}