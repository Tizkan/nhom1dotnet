using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    public class Customer
    {
        public int id { get; set; }

        public string full_name { get; set; }

        public string phone { get; set; }

        public string email { get; set; }

        public string address { get; set; }

        public string citizen_id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? created_at { get; set; }
    }
}