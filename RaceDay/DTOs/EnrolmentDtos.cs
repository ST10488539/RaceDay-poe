using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class EnrolRequestDto
    {
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }

    public class EnrolmentDto
    {
        public int RegistrationID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? RaceNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationStatus { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
    }
}