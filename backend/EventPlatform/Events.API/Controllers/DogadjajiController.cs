using DTO.Lokacije;
using DTO.Predavaci;
using DTO.StrucniDogadjaji;
using DTO.TipoviDogadjaja;
using Events.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Events.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DogadjajiController : ControllerBase
    {
        private static int _counter = 0;
        private readonly EventContext Context;
        private readonly ILogger<DogadjajiController> logger;

        public DogadjajiController(EventContext Context, ILogger<DogadjajiController> logger)
        {
            this.Context = Context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StrucniDogadjajDTO>>> Get()
        {
            _counter++;
            logger.LogInformation($"[Events API] Zahtev broj: {_counter}");


            if (_counter % 4 != 0)
            {
                logger.LogWarning($"Simulirana greska 500 na pokusaju: {_counter}");
                return StatusCode(500, "Simulated server error");
            }

            var dogadjaji = await Context.StrucniDogadjaji
                               .Include(sd => sd.Predavaci)
                               .Include(sd => sd.Lokacija)
                               .Include(sd => sd.TipDogadjaja)
                               .ToListAsync();

            var dogadjajiDTO = dogadjaji.Select(sd => new StrucniDogadjajDTO
            {
                StrucniDogadjajID = sd.StrucniDogadjajID,
                Naziv = sd.Naziv,
                Agenda = sd.Agenda,
                DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja,
                Trajanje = sd.Trajanje,
                CenaKotizacije = sd.CenaKotizacije,
                Lokacija = new LokacijaDTO
                {
                    LokacijaID = sd.LokacijaID,
                    Naziv = sd.Lokacija.Naziv,
                    Adresa = sd.Lokacija.Adresa,
                    Kapacitet = sd.Lokacija.Kapacitet
                },
                Predavaci = sd.Predavaci.Select(p => new PredavacDTO
                {
                    PredavacID = p.PredavacID,
                    Ime = p.Ime,
                    Prezime = p.Prezime,
                    Titula = p.Titula,
                    OblastStrucnosti = p.OblastStrucnosti
                }).ToList(),
                TipDogadjaja = new TipDogadjajaDTO
                {
                    TipDogadjajaID = sd.TipDogadjajaID,
                    NazivTipa = sd.TipDogadjaja.NazivTipa
                }
            }).ToList();

            logger.LogInformation("Uspesno vraceni dogadjaji!");
            return Ok(dogadjajiDTO);
        }
    }
}

