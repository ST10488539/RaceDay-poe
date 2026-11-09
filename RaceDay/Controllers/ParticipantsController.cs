using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllParticipants()
        {
            var participants = _context.Participants.ToList();
            return Ok(participants);
        }

        [HttpGet("{id}")]
        public IActionResult GetParticipantById(int id)
        {
            var participant = _context.Participants.Find(id);
            if (participant == null)
            {
                return NotFound();
            }
            return Ok(participant);
        }

        [HttpPost]
        public IActionResult CreateParticipant([FromBody] Participant participant)
        {
            if (participant == null)
            {
                return BadRequest();
            }

            _context.Participants.Add(participant);
            _context.SaveChanges();

            // Note the capital 'P' in ParticipantID below
            return CreatedAtAction(nameof(GetParticipantById), new { id = participant.ParticipantID }, participant);
        }
    }
}