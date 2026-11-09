using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class CategoryRequestDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        [Range(0, 120)]
        public int MinAge { get; set; }

        /// <summary>Leave out for no upper age limit.</summary>
        [Range(0, 120)]
        public int? MaxAge { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Description { get; set; }
    }
}