using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    [Table("staffs")]
    public class Staff
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string full_name { get; set; } = "";

        public DateOnly? birth_date { get; set; }

        [Required]
        public string email { get; set; } = "";

        public string? citizen_id { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
    }
}
