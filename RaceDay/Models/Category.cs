using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        public int MinAge { get; set; }
        public int? MaxAge { get; set; }  

        [MaxLength(500)]
        public string? Description { get; set; }

        public Event Event { get; set; } = null!;
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}