using System.ComponentModel.DataAnnotations.Schema;

namespace nhom1dotnet.Models
{
    public class Service
    {
        public int id { get; set; }

        public string service_name { get; set; }

        public decimal? price { get; set; }

        public string description { get; set; }
    }
}