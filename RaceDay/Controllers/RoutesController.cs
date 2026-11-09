using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RoutesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllRoutes()
        {
            var routes = _context.Routes.ToList();
            return Ok(routes);
        }

        [HttpGet("{id}")]
        public IActionResult GetRouteById(int id)
        {
            var route = _context.Routes.Find(id);
            if (route == null)
            {
                return NotFound();
            }
            return Ok(route);
        }

        [HttpPost]
        public IActionResult CreateRoute([FromBody] RaceDay.Models.Route route)
        {
            if (route == null)
            {
                return BadRequest();
            }

            _context.Routes.Add(route);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetRouteById), new { id = route.RouteID }, route);
        }
    }
}