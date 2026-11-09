using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class ResultUpdateDto
    {
        /// <summary>Format hh:mm:ss, for example 00:52:34</summary>
        public TimeSpan FinishTime { get; set; }

        /// <summary>Optional. Format hh:mm:ss. Used for the pace when given.</summary>
        public TimeSpan? ChipTime { get; set; }

        [Range(1, int.MaxValue)]
        public int OverallPosition { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryPosition { get; set; }

        /// <summary>OFFICIAL, PROVISIONAL or DISQUALIFIED (defaults to OFFICIAL)</summary>
        public string? ResultStatus { get; set; }
    }

    public class ResultCreateDto : ResultUpdateDto
    {
        [Range(1, int.MaxValue)]
        public int RegistrationID { get; set; }
    }

    public class ResultDto
    {
        public int ResultID { get; set; }
        public int RegistrationID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? RaceNumber { get; set; }
        public TimeSpan FinishTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }
        public decimal Pace { get; set; }
        public string ResultStatus { get; set; } = string.Empty;
    }
}