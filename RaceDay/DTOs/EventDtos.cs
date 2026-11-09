using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class EventRequestDto
    {
        [Required, MaxLength(200)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        /// <summary>RUN, WALK or CYCLE</summary>
        [Required]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageURL { get; set; }
    }

    public class EventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? ImageURL { get; set; }
        public int OrganiserID { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
    }
}