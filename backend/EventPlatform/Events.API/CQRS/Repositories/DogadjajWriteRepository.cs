using Events.API.CQRS.Commands;
using Events.API.Data;
using Events.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Events.API.CQRS.Repositories
{
    public class DogadjajWriteRepository : IDogadjajWriteRepository
    {
        private readonly EventContext _context;

        public DogadjajWriteRepository(EventContext context)
        {
            _context = context;
        }

        public async Task<int> AddDogadjajAsync(AddDogadjajCommand command, int maksimalanKapacitet, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var predavaci = await _context.Predavaci
                    .Where(p => command.PredavaciIDs.Contains(p.PredavacID))
                    .ToListAsync(cancellationToken);

                var dogadjaj = new StrucniDogadjaj
                {
                    Naziv = command.Naziv?.Trim() ?? string.Empty,
                    Agenda = command.Agenda?.Trim() ?? string.Empty,
                    DatumVremeOdrzavanja = command.DatumVremeOdrzavanja,
                    Trajanje = command.Trajanje,
                    CenaKotizacije = command.CenaKotizacije,
                    LokacijaID = command.LokacijaID,
                    Predavaci = predavaci,
                    TipDogadjajaID = command.TipDogadjajaID,
                    MaksimalanKapacitet = maksimalanKapacitet,
                    SlobodnaMesta = maksimalanKapacitet
                };

                _context.StrucniDogadjaji.Add(dogadjaj);
                await _context.SaveChangesAsync(cancellationToken);

                var payload = JsonSerializer.Serialize(new
                {
                    StrucniDogadjajID = dogadjaj.StrucniDogadjajID,
                    Naziv = dogadjaj.Naziv
                });

                _context.OutboxMessages.Add(new OutboxMessage
                {
                    EventType = "DogadjajKreiran",
                    Payload = payload,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return dogadjaj.StrucniDogadjajID;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> EditDogadjajAsync(EditDogadjajCommand command, int maksimalanKapacitet, CancellationToken cancellationToken)
        {
            var dogadjaj = await _context.StrucniDogadjaji
                .Include(sd => sd.Predavaci)
                .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == command.StrucniDogadjajID, cancellationToken);

            if (dogadjaj is null)
            {
                return false;
            }

            var predavaci = await _context.Predavaci
                .Where(p => command.PredavaciIDs.Contains(p.PredavacID))
                .ToListAsync(cancellationToken);

            dogadjaj.Naziv = command.Naziv?.Trim() ?? string.Empty;
            dogadjaj.Agenda = command.Agenda?.Trim() ?? string.Empty;
            dogadjaj.DatumVremeOdrzavanja = command.DatumVremeOdrzavanja;
            dogadjaj.Trajanje = command.Trajanje;
            dogadjaj.CenaKotizacije = command.CenaKotizacije;
            dogadjaj.LokacijaID = command.LokacijaID;
            dogadjaj.TipDogadjajaID = command.TipDogadjajaID;
            dogadjaj.MaksimalanKapacitet = maksimalanKapacitet;
            dogadjaj.SlobodnaMesta = Math.Min(dogadjaj.SlobodnaMesta, maksimalanKapacitet);

            dogadjaj.Predavaci.Clear();
            foreach (var predavac in predavaci)
            {
                dogadjaj.Predavaci.Add(predavac);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteDogadjajAsync(int strucniDogadjajID, CancellationToken cancellationToken)
        {
            var dogadjaj = await _context.StrucniDogadjaji
                .Include(sd => sd.Predavaci)
                .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == strucniDogadjajID, cancellationToken);

            if (dogadjaj is null)
            {
                return false;
            }

            dogadjaj.Predavaci.Clear();
            _context.StrucniDogadjaji.Remove(dogadjaj);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
