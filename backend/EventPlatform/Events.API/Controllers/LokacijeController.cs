using DTO.Lokacije;
using Events.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Events.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LokacijeController : ControllerBase
    {
        private readonly EventContext _context;

        public LokacijeController(EventContext Context)
        {
            _context = Context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LokacijaDTO>>> Get()
        {
            var lokacije = await _context.Lokacije.ToListAsync();

            var rezultat = lokacije.Select(l => new LokacijaDTO
            {
                LokacijaID = l.LokacijaID,
                Naziv = l.Naziv,
                Adresa = l.Adresa,
                Kapacitet = l.Kapacitet
            }).ToList();

            return Ok(rezultat);
        }
    }
}
