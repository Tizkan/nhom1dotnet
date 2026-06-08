using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    public class Bookings
    {
        public int id { get; set; }

        [ForeignKey("Customer")]
        public int customer_id { get; set; }

        [ForeignKey("Room")]
        public int room_id { get; set; }
        public DateTime check_in { get; set; }

        public DateTime check_out { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        public int adults { get; set; }

        public int children { get; set; }

        public decimal? total_amount { get; set; }

        public string status { get; set; }

        public Customer Customer { get; set; }

        public Room Room { get; set; }
    }
}