using DTO.Lokacije;
using DTO.StrucniDogadjaji;
using Events.API.Data;
using Events.API.Models;
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
            var lokacije = await _context.Lokacije
                .Include(l => l.StrucniDogadjaji)
                .ToListAsync();

            var rezultat = lokacije.Select(l => new LokacijaDTO
            {
                LokacijaID = l.LokacijaID,
                Naziv = l.Naziv,
                Adresa = l.Adresa,
                Kapacitet = l.Kapacitet,
                StrucniDogadjaji = l.StrucniDogadjaji.Select(sd => new StrucniDogadjajDTO
                {
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja,
                    Trajanje = sd.Trajanje
                }).ToList()
            }).ToList();

            return Ok(rezultat);
        }
        [HttpPost]
        public async Task<ActionResult<int>> Create(LokacijaCreateDTO request)
        {
            var novaLokacija = new Lokacija
            {
                Naziv = request.Naziv,
                Adresa = request.Adresa,
                Kapacitet = request.Kapacitet
            };
            _context.Add(novaLokacija);

            await _context.SaveChangesAsync();

            return Ok(novaLokacija.LokacijaID);
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<LokacijaDTO>> GetById(int Id)
        {
            var lokacija = await _context.Lokacije
                            .Include(l => l.StrucniDogadjaji)
                            .FirstOrDefaultAsync(l => l.LokacijaID == Id);

            if(lokacija == null)
            {
                return NotFound();
            }

            var rezultat = new LokacijaDTO
            {
                LokacijaID = lokacija.LokacijaID,
                Naziv = lokacija.Naziv,
                Adresa = lokacija.Adresa,
                Kapacitet = lokacija.Kapacitet,
                StrucniDogadjaji = lokacija.StrucniDogadjaji.Select(sd => new StrucniDogadjajDTO
                {
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja,
                    Trajanje = sd.Trajanje
                }).ToList()
            };

            return Ok(rezultat);

        }
            
        [HttpPut]
        public async Task<ActionResult> Edit(LokacijaCreateDTO request)
        {
            var lokacija = await _context.Lokacije
                            .FirstOrDefaultAsync(l => l.LokacijaID == request.LokacijaID);

            if(lokacija == null)
            {
                return NotFound();
            }

            lokacija.Naziv = request.Naziv;
            lokacija.Adresa = request.Adresa;
            lokacija.Kapacitet = request.Kapacitet;

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var lokacija = await _context.Lokacije
                            .Include(l => l.StrucniDogadjaji)
                            .FirstOrDefaultAsync(l => l.LokacijaID == Id);

            if(lokacija == null)
            {
                return NotFound();
            }

            lokacija.StrucniDogadjaji.Clear();

            _context.Lokacije.Remove(lokacija);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
