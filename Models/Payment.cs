using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    public class Payment
    {
        public int id { get; set; }

        public int booking_id { get; set; }

        public DateTime payment_date { get; set; }

        public decimal amount { get; set; }

        public string method { get; set; }

        public string status { get; set; }

        public Bookings Booking { get; set; }
    }
}