using DTO.Predavaci;
using Events.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Events.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PredavaciController : ControllerBase
    {
        private readonly EventContext _context;
        public PredavaciController(EventContext Context)
        {
            _context = Context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredavacDTO>>> Get()
        {
            var predavaci = await _context.Predavaci.Select(p => new PredavacDTO
            {
                PredavacID = p.PredavacID,
                Ime = p.Ime,
                Prezime = p.Prezime,
                Titula = p.Titula,
                OblastStrucnosti = p.OblastStrucnosti
            }).ToListAsync();

            return Ok(predavaci);
        }
    }
}
