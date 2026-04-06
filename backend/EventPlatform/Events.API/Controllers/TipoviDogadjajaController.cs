using DTO.StrucniDogadjaji;
using DTO.TipoviDogadjaja;
using Events.API.Data;
using Events.API.Models;
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
            var tipoviDogadjaja = await _context.TipoviDogadjaja
                .Include(td => td.StrucniDogadjaji)
                .Select(td => new TipDogadjajaDTO
                {
                    TipDogadjajaID = td.TipDogadjajaID,
                    NazivTipa = td.NazivTipa,
                    StrucniDogadjaji = td.StrucniDogadjaji.Select(sd => new StrucniDogadjajDTO
                    {
                        StrucniDogadjajID = sd.StrucniDogadjajID,
                        Naziv = sd.Naziv,
                        DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja
                    }).ToList()
                }).ToListAsync();

            return Ok(tipoviDogadjaja);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipDogadjajaDTO>> GetById(int id)
        {
            var tip = await _context.TipoviDogadjaja
                .Include(td => td.StrucniDogadjaji)
                .FirstOrDefaultAsync(td => td.TipDogadjajaID == id);

            if (tip == null) return NotFound();

            var dto = new TipDogadjajaDTO
            {
                TipDogadjajaID = tip.TipDogadjajaID,
                NazivTipa = tip.NazivTipa,
                StrucniDogadjaji = tip.StrucniDogadjaji.Select(sd => new StrucniDogadjajDTO
                {
                    StrucniDogadjajID = sd.StrucniDogadjajID,
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(TipDogadjajaCreateDTO request)
        {
            var noviTip = new TipDogadjaja
            {
                NazivTipa = request.NazivTipa
            };

            _context.TipoviDogadjaja.Add(noviTip);
            await _context.SaveChangesAsync();

            return Ok(noviTip.TipDogadjajaID);
        }

        [HttpPut]
        public async Task<ActionResult> Edit(TipDogadjajaCreateDTO request)
        {
            var tip = await _context.TipoviDogadjaja
                .FirstOrDefaultAsync(td => td.TipDogadjajaID == request.TipDogadjajaID);

            if (tip == null) return NotFound();

            tip.NazivTipa = request.NazivTipa;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var tip = await _context.TipoviDogadjaja.FirstOrDefaultAsync(td => td.TipDogadjajaID == id);
            if (tip == null) return NotFound();

            _context.TipoviDogadjaja.Remove(tip);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
