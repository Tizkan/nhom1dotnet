using System.ComponentModel.DataAnnotations.Schema;
namespace nhom1dotnet.Models
{
    public class Room
    {
        public int id { get; set; }

        public string room_number { get; set; }

        [ForeignKey("RoomType")]
        public int room_type_id { get; set; }

        public int floor_number { get; set; }

        public string status { get; set; }

        public RoomType RoomType { get; set; }
    }
}