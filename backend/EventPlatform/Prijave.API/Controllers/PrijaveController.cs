using DTO.Prijave;
using DTO.Ucesnici;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;
using Prijave.API.HostedServices;
using Prijave.API.Models;
using System.Runtime.InteropServices;

namespace Prijave.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrijaveController : ControllerBase
    {
        public PrijavaContext _context { get; set; }
        private readonly DogadjajDetaljiClient _klijent;
        public PrijaveController(PrijavaContext context, DogadjajDetaljiClient klijent)
        {
            _context = context;
            _klijent = klijent;
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

        [HttpPost("TestirajRequestReply/{dogadjajId}")]
        public async Task<IActionResult> TestirajRequestReply(int dogadjajId)
        {
            var zahtev = new DogadjajDetaljiRequest(dogadjajId);

            await _klijent.PosaljiZahtevAsync(zahtev);

            return Ok("Zahtev poslat! Pogledaj obe crne konzole (Events i Prijave) da pratiš šta se dešava!");
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
                   UcesnikID = p.UcesnikID,
                   Ime = p.Ucesnik.Ime,
                   Prezime = p.Ucesnik.Prezime,
                   Email = p.Ucesnik.Email
                },
                StrucniDogadjajID = p.StrucniDogadjajID,
                DatumPrijave = p.DatumPrijave
            }).ToList();

            return Ok(rezultat);
        }
        [HttpGet("{ucesnikId}/{dogadjajId}")]
        public async Task<ActionResult<PrijavaDTO>> GetById(int ucesnikId, int dogadjajId)
        {
            var prijava = await _context.Prijave
                                .Include(p => p.Ucesnik)
                                .FirstOrDefaultAsync(p => p.UcesnikID == ucesnikId && p.StrucniDogadjajID == dogadjajId);

            if(prijava == null)
            {
                return NotFound();
            }

            var dto = new PrijavaDTO
            {
                Ucesnik = new UcesnikDTO
                {
                    UcesnikID = prijava.UcesnikID,
                    Ime = prijava.Ucesnik.Ime,
                    Prezime = prijava.Ucesnik.Prezime,
                    Email = prijava.Ucesnik.Email
                },
                StrucniDogadjajID = prijava.StrucniDogadjajID
            };

            return Ok(dto);
        }

    }
}
