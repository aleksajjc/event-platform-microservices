using DTO.Lokacije;
using DTO.Predavaci;
using DTO.StrucniDogadjaji;
using DTO.TipoviDogadjaja;
using Events.API.Data;
using Events.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        [HttpPost]
        public async Task<ActionResult<int>> Create(StrucniDogadjajCreateDTO request)
        {
            var noviDogadjaj = new StrucniDogadjaj
            {
                Naziv = request.Naziv,
                Agenda = request.Agenda,
                DatumVremeOdrzavanja = request.DatumVremeOdrzavanja,
                Trajanje = request.Trajanje,
                CenaKotizacije = request.CenaKotizacije,
                LokacijaID = request.LokacijaID,
                Predavaci = Context.Predavaci
                    .Where(p => request.PredavaciIDs.Contains(p.PredavacID))
                    .ToList(),
                TipDogadjajaID = request.TipDogadjajaID
            };

            Context.Add(noviDogadjaj);
            await Context.SaveChangesAsync();

            return Ok(noviDogadjaj.StrucniDogadjajID);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StrucniDogadjajDTO>> GetById(int id)
        {
            _counter++;

            if (_counter % 4 != 0)
            {
                return StatusCode(500, "Simulated error on Details!");
            }

            var dogadjaj = await Context.StrucniDogadjaji
                .Include(sd => sd.Lokacija)
                .Include(sd => sd.TipDogadjaja)
                .Include(sd => sd.Predavaci)
                .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == id);

            if (dogadjaj == null) return NotFound();

            var dto = new StrucniDogadjajDTO
            {
                StrucniDogadjajID = dogadjaj.StrucniDogadjajID,
                Naziv = dogadjaj.Naziv,
                Agenda = dogadjaj.Agenda,
                DatumVremeOdrzavanja = dogadjaj.DatumVremeOdrzavanja,
                Trajanje = dogadjaj.Trajanje,
                CenaKotizacije = dogadjaj.CenaKotizacije,
                Lokacija = new LokacijaDTO
                {
                    LokacijaID = dogadjaj.Lokacija.LokacijaID,
                    Naziv = dogadjaj.Lokacija.Naziv,
                    Adresa = dogadjaj.Lokacija.Adresa,
                    Kapacitet = dogadjaj.Lokacija.Kapacitet
                },
                TipDogadjaja = new TipDogadjajaDTO
                {
                    TipDogadjajaID = dogadjaj.TipDogadjaja.TipDogadjajaID,
                    NazivTipa = dogadjaj.TipDogadjaja.NazivTipa
                },
                Predavaci = dogadjaj.Predavaci.Select(p => new PredavacDTO
                {
                    PredavacID = p.PredavacID,
                    Ime = p.Ime,
                    Prezime = p.Prezime,
                    Titula = p.Titula,
                    OblastStrucnosti = p.OblastStrucnosti
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult> Edit(StrucniDogadjajCreateDTO request)
        {
            var dogadjaj = await Context.StrucniDogadjaji
                            .Include(sd => sd.Predavaci)
                            .Where(sd => sd.StrucniDogadjajID == request.StrucniDogadjajID)
                            .FirstOrDefaultAsync();

            if (dogadjaj == null)
            {
                return NotFound();
            }

            dogadjaj.Naziv = request.Naziv;
            dogadjaj.Agenda = request.Agenda;
            dogadjaj.DatumVremeOdrzavanja = request.DatumVremeOdrzavanja;
            dogadjaj.Trajanje = request.Trajanje;
            dogadjaj.CenaKotizacije = request.CenaKotizacije;
            dogadjaj.LokacijaID = request.LokacijaID;
            dogadjaj.Predavaci.Clear();
            dogadjaj.Predavaci = Context.Predavaci
                                  .Where(p => request.PredavaciIDs.Contains(p.PredavacID))
                                  .ToList();
            dogadjaj.TipDogadjajaID = request.TipDogadjajaID;

            await Context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var dogadjaj = await Context.StrucniDogadjaji
                                .Include(sd => sd.Predavaci)
                                .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == Id);

            if(dogadjaj == null)
            {
                return NotFound();
            }
            dogadjaj.Predavaci.Clear();

            Context.Remove(dogadjaj);
            await Context.SaveChangesAsync();
            return Ok();
        }
    }
}

