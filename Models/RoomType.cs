using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    [Table("room_types")]
    public class RoomType
    {
        public int id { get; set; }

        public string name { get; set; }

        public decimal price { get; set; }

        public int capacity { get; set; }

        public string description { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
}