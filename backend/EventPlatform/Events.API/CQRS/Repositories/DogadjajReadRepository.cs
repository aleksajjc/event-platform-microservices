using Events.API.CQRS.ReadModels;
using Events.API.Data;
using Events.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.API.CQRS.Repositories
{
    public class DogadjajReadRepository : IDogadjajReadRepository
    {
        private readonly EventContext _context;

        public DogadjajReadRepository(EventContext context)
        {
            _context = context;
        }

        public async Task<List<DogadjajReadModel>> GetAllDogadjajiAsync(CancellationToken cancellationToken)
        {
            
            var dogadjaji = await _context.StrucniDogadjaji
                .AsNoTracking()
                .AsSplitQuery()
                .Include(sd => sd.Lokacija)
                .Include(sd => sd.TipDogadjaja)
                .Include(sd => sd.Predavaci)
                .OrderBy(sd => sd.DatumVremeOdrzavanja)
                .ToListAsync(cancellationToken);

            return dogadjaji.Select(Map).ToList();
        }

        public async Task<DogadjajReadModel?> GetDogadjajByIdAsync(int strucniDogadjajID, CancellationToken cancellationToken)
        {
            var dogadjaj = await _context.StrucniDogadjaji
                .AsNoTracking()
                .AsSplitQuery()
                .Include(sd => sd.Lokacija)
                .Include(sd => sd.TipDogadjaja)
                .Include(sd => sd.Predavaci)
                .FirstOrDefaultAsync(sd => sd.StrucniDogadjajID == strucniDogadjajID, cancellationToken);

            return dogadjaj is null ? null : Map(dogadjaj);
        }

        public async Task<List<DogadjajReadModel>> GetDogadjajiByLokacijaAsync(int lokacijaID, CancellationToken cancellationToken)
        {
            var dogadjaji = await _context.StrucniDogadjaji
                .AsNoTracking()
                .AsSplitQuery()
                .Include(sd => sd.Lokacija)
                .Include(sd => sd.TipDogadjaja)
                .Include(sd => sd.Predavaci)
                .Where(sd => sd.LokacijaID == lokacijaID)
                .OrderBy(sd => sd.DatumVremeOdrzavanja)
                .ToListAsync(cancellationToken);

            return dogadjaji.Select(Map).ToList();
        }

        private static DogadjajReadModel Map(StrucniDogadjaj dogadjaj)
        {
            return new DogadjajReadModel
            {
                StrucniDogadjajID = dogadjaj.StrucniDogadjajID,
                Naziv = dogadjaj.Naziv ?? string.Empty,
                Agenda = dogadjaj.Agenda ?? string.Empty,
                DatumVremeOdrzavanja = dogadjaj.DatumVremeOdrzavanja,
                Trajanje = dogadjaj.Trajanje,
                CenaKotizacije = dogadjaj.CenaKotizacije,
                MaksimalanKapacitet = dogadjaj.MaksimalanKapacitet,
                SlobodnaMesta = dogadjaj.SlobodnaMesta,
                Lokacija = dogadjaj.Lokacija is null
                    ? new LokacijaReadModel()
                    : new LokacijaReadModel
                    {
                        LokacijaID = dogadjaj.Lokacija.LokacijaID,
                        Naziv = dogadjaj.Lokacija.Naziv ?? string.Empty,
                        Adresa = dogadjaj.Lokacija.Adresa ?? string.Empty,
                        Kapacitet = dogadjaj.Lokacija.Kapacitet
                    },
                TipDogadjaja = dogadjaj.TipDogadjaja is null
                    ? new TipDogadjajaReadModel()
                    : new TipDogadjajaReadModel
                    {
                        TipDogadjajaID = dogadjaj.TipDogadjaja.TipDogadjajaID,
                        NazivTipa = dogadjaj.TipDogadjaja.NazivTipa ?? string.Empty
                    },
                Predavaci = dogadjaj.Predavaci?.Select(MapPredavac).ToList() ?? new List<PredavacReadModel>()
            };
        }

        private static PredavacReadModel MapPredavac(Predavac predavac)
        {
            return new PredavacReadModel
            {
                PredavacID = predavac.PredavacID,
                Ime = predavac.Ime ?? string.Empty,
                Prezime = predavac.Prezime ?? string.Empty,
                Titula = predavac.Titula ?? string.Empty,
                OblastStrucnosti = predavac.OblastStrucnosti ?? string.Empty
            };
        }
    }
}
