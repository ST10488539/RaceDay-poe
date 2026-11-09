using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required, MaxLength(200)]
        public string EventName { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        public int OrganiserID { get; set; }

        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;   // RUN, WALK or CYCLE

        [MaxLength(500)]
        public string? ImageURL { get; set; }

        public User Organiser { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Route> Routes { get; set; } = new List<Route>();
        public ICollection<Weather> WeatherForecasts { get; set; } = new List<Weather>();
    }
}