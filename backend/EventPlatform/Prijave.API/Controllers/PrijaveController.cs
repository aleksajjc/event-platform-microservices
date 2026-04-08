using DTO.Prijave;
using DTO.Ucesnici;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.Models;

namespace Prijave.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrijaveController : ControllerBase
    {
        public PrijavaContext _context { get; set; }
        public PrijaveController(PrijavaContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<ActionResult<int>> Create(PrijavaCreateDTO request)
        {
            var novaPrijava = new Prijava
            {
                Ucesnik = new Ucesnik
                {
                    Ime = request.Ime,
                    Prezime = request.Prezime,
                    Email = request.Email
                },
                StrucniDogadjajID = request.StrucniDogadjajID,
                DatumPrijave = request.DatumPrijave
            };

            _context.Prijave.Add(novaPrijava);
            await _context.SaveChangesAsync();

            return Ok($"{novaPrijava.UcesnikID} {novaPrijava.StrucniDogadjajID}");
        }

        [HttpGet]
        public async Task<ActionResult<List<PrijavaDTO>>> Get()
        {
            var prijave = await _context.Prijave
                .Include(p => p.Ucesnik)
                .ToListAsync();

            var rezultat = prijave.Select(p => new PrijavaDTO
            {
                Ucesnik = new UcesnikDTO
                {
                   Ime = p.Ucesnik.Ime,
                   Prezime = p.Ucesnik.Prezime,
                   Email = p.Ucesnik.Email
                },
                StrucniDogadjajID = p.StrucniDogadjajID,
                DatumPrijave = p.DatumPrijave
            }).ToList();

            return Ok(rezultat);
        }
    }
}
