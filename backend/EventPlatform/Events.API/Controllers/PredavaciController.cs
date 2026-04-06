using DTO.Predavaci;
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
                OblastStrucnosti = p.OblastStrucnosti,
                StrucniDogadjaji = p.StrucniDogadjaji.Select(sd => new StrucniDogadjajDTO
                {
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja
                }).ToList()
            }).ToListAsync();

            return Ok(predavaci);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(PredavacCreateDTO request)
        {
            var noviPredavac = new Predavac
            {
                Ime = request.Ime,
                Prezime = request.Prezime,
                Titula = request.Titula,
                OblastStrucnosti = request.OblastStrucnosti
            };

            _context.Add(noviPredavac);
            await _context.SaveChangesAsync();

            return Ok(noviPredavac.PredavacID);
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<PredavacDTO>> GetById(int Id)
        {
            var predavac = await _context.Predavaci
                                 .Include(p => p.StrucniDogadjaji)
                                .FirstOrDefaultAsync(p => p.PredavacID == Id);

            if(predavac == null)
            {
                return NotFound();
            }

            var dto = new PredavacDTO
            {
                PredavacID = predavac.PredavacID,
                Ime = predavac.Ime,
                Prezime = predavac.Prezime,
                Titula = predavac.Titula,
                OblastStrucnosti = predavac.OblastStrucnosti,
                StrucniDogadjaji = predavac.StrucniDogadjaji.Select(p => new StrucniDogadjajDTO
                {
                    Naziv = p.Naziv,
                    DatumVremeOdrzavanja = p.DatumVremeOdrzavanja
                }).ToList()
            };
            return Ok(dto);
        }
        [HttpPut]
        public async Task<ActionResult> Edit(PredavacCreateDTO request)
        {
            var predavac = await _context.Predavaci
                            .FirstOrDefaultAsync(p => p.PredavacID == request.PredavacID);

            if(predavac == null)
            {
                return NotFound();
            }

            predavac.Ime = request.Ime;
            predavac.Prezime = request.Prezime;
            predavac.Titula = request.Titula;
            predavac.OblastStrucnosti = request.OblastStrucnosti;

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var predavac = await _context.Predavaci
                                .FirstOrDefaultAsync(p => p.PredavacID == Id);

            if(predavac == null)
            {
                return NotFound();
            }

           
            _context.Predavaci.Remove(predavac);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
