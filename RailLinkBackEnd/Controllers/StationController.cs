using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.Supabase;

namespace RailLinkBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllResult()
        {
            var stations = await _context.Histories.ToListAsync();
            return Ok(stations);
        }

    }
}
