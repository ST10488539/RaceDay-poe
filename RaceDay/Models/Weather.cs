using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Weather
    {
        public int WeatherID { get; set; }
        public int EventID { get; set; }

        [Column(TypeName = "date")]
        public DateTime ForecastDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Temperature { get; set; }

        [Required, MaxLength(100)]
        public string Condition { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal WindSpeed { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Humidity { get; set; }

        [MaxLength(100)]
        public string? Source { get; set; }

        public Event Event { get; set; } = null!;
    }
}