using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public WeatherController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllWeather()
        {
            var weather = _context.Weather.ToList();
            return Ok(weather);
        }

        [HttpGet("{id}")]
        public IActionResult GetWeatherById(int id)
        {
            var weather = _context.Weather.Find(id);
            if (weather == null)
            {
                return NotFound();
            }
            return Ok(weather);
        }

        [HttpPost]
        public IActionResult CreateWeather([FromBody] Weather weather)
        {
            if (weather == null)
            {
                return BadRequest();
            }

            _context.Weather.Add(weather);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetWeatherById), new { id = weather.WeatherID }, weather);
        }
    }
}