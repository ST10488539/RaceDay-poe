using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public int EventID { get; set; }

        [Required, MaxLength(200)]
        public string RouteName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal ElevationGain { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? RouteMapURL { get; set; }

        public Event Event { get; set; } = null!;
    }
}