using DTO.TipoviDogadjaja;
using Events.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Events.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TipoviDogadjajaController : ControllerBase
    {
        private readonly EventContext _context;
        public TipoviDogadjajaController(EventContext Context)
        {
            _context = Context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipDogadjajaDTO>>> Get()
        {
            var tipoviDogadjaja = await _context.TipoviDogadjaja.Select(td => new TipDogadjajaDTO
            {
                TipDogadjajaID = td.TipDogadjajaID,
                NazivTipa = td.NazivTipa
            }).ToListAsync();

            return Ok(tipoviDogadjaja);
        }
    }
}
