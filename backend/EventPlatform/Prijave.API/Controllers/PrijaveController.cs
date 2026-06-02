using DTO.Prijave;
using DTO.Ucesnici;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Background_services;
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
        private readonly EmailPublisher _emailPublisher;
        public PrijaveController(PrijavaContext context, DogadjajDetaljiClient klijent, EmailPublisher emailPublisher)
        {
            _context = context;
            _klijent = klijent;
            _emailPublisher = emailPublisher;
        }


        [HttpPost]
        public async Task<ActionResult<int>> Create(PrijavaCreateDTO request)
        {
            var correlationId = Guid.NewGuid();
            var novaPrijava = new Prijava
            {
                Ucesnik = new Ucesnik
                {
                    Ime = request.Ime,
                    Prezime = request.Prezime,
                    Email = request.Email
                },
                StrucniDogadjajID = request.StrucniDogadjajID,
                DatumPrijave = request.DatumPrijave,
                StatusPrijava = StatusPrijava.NaCekanju,
                CorrelationID = correlationId,
                CenaKotizacije = request.CenaKotizacije
            };

            var outboxMessage = new PrijavaZapocetaOutboxMessage
            {
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.ForProcessing,
                CreatedAt = DateTime.UtcNow
            };

            _context.Prijave.Add(novaPrijava);
            _context.PrijavaZapocetaOutboxMessages.Add(outboxMessage);
            await _context.SaveChangesAsync();

            return Ok($"{novaPrijava.UcesnikID} {novaPrijava.StrucniDogadjajID}");
        }
        [HttpPost("TestirajEmail")]
        public async Task<IActionResult> TestirajEmail()
        {
            for (int i = 1; i <= 13; i++)
            {
                var mejl = new EmailMessage($"{i}@gmail.com", $"Potvrda prijave #{i}", $"Tekst:{i}");
                await _emailPublisher.PosaljiEmailNaQueue(mejl);
            }
            return Ok("12 mejlova poslato u Queue! Pogledaj konzolu i outbox folder.");
        }

        [HttpPost("TestirajRequestReply/{dogadjajId}")]
        public async Task<IActionResult> TestirajRequestReply(int dogadjajId)
        {
            var zahtev = new DogadjajDetaljiRequest(dogadjajId);

            await _klijent.PosaljiZahtevAsync(zahtev);

            return Ok("Zahtev poslat! Pogledaj obe crne konzole (Events i Prijave) da pratite šta se dešava!");
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
                DatumPrijave = p.DatumPrijave,
                StatusPrijava = p.CorrelationID == Guid.Empty ? "Potvrdjena" : p.StatusPrijava.ToString(),
                CorrelationID = p.CorrelationID
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
                StrucniDogadjajID = prijava.StrucniDogadjajID,
                DatumPrijave = prijava.DatumPrijave,
                StatusPrijava = prijava.CorrelationID == Guid.Empty ? "Potvrdjena" : prijava.StatusPrijava.ToString(),
                CorrelationID = prijava.CorrelationID
            };

            return Ok(dto);
        }

    }
}
