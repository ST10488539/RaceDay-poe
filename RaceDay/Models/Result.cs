using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int RegistrationID { get; set; }

        public TimeSpan FinishTime { get; set; }
        public TimeSpan? ChipTime { get; set; }

        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Pace { get; set; }

        [Required, MaxLength(30)]
        public string ResultStatus { get; set; } = "OFFICIAL";

        public Registration Registration { get; set; } = null!;
    }
}