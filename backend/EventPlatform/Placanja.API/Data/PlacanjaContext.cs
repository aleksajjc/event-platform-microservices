using Microsoft.EntityFrameworkCore;
using Placanja.API.Models;

namespace Placanja.API.Data
{
    public class PlacanjaContext : DbContext
    {
        public PlacanjaContext(DbContextOptions<PlacanjaContext> options) : base(options)
        {
        }

        public DbSet<RacunUcesnika> RacuniUcesnika { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed podaci za testiranje
            modelBuilder.Entity<RacunUcesnika>().HasData(
                new RacunUcesnika
                {
                    UcesnikID = 1,
                    Ime = "Aleksa",
                    Prezime = "Jovanovic",
                    Email = "aleksa@example.com",
                    StanjeNaRacunu = 5000.00
                },
                new RacunUcesnika
                {
                    UcesnikID = 2,
                    Ime = "Marko",
                    Prezime = "Markovic",
                    Email = "marko@example.com",
                    StanjeNaRacunu = 50.00 // Nedovoljno novca za kotizaciju!
                }
            );
        }
    }
}
