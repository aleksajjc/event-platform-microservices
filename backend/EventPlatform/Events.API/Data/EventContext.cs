using EventPlatform.Domen;
using Microsoft.EntityFrameworkCore;

namespace Events.API.Data
{
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions<EventContext> options) : base(options)
        {
        }

        public DbSet<Lokacija> Lokacije { get; set; }
        public DbSet<Predavac> Predavaci { get; set; }
        public DbSet<StrucniDogadjaj> StrucniDogadjaji { get; set; }
        public DbSet<TipDogadjaja> TipoviDogadjaja { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StrucniDogadjaj>()
               .HasOne(sd => sd.Lokacija)
               .WithMany(l => l.StrucniDogadjaji)
               .HasForeignKey(sd => sd.LokacijaID)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StrucniDogadjaj>()
                .HasMany(sd => sd.Predavaci)
                .WithMany(p => p.StrucniDogadjaji)
                .UsingEntity<Dictionary<string, object>>(
                    "StrucniDogadjajiPredavaci",
                    t => t.HasOne<Predavac>().WithMany().HasForeignKey("PredavacID").OnDelete(DeleteBehavior.Restrict),
                    t => t.HasOne<StrucniDogadjaj>().WithMany().HasForeignKey("StrucniDogadjajID").OnDelete(DeleteBehavior.Restrict)
                );
            modelBuilder.Entity<StrucniDogadjaj>()
                .HasOne(sd => sd.TipDogadjaja)
                .WithMany(tip => tip.StrucniDogadjaji)
                .HasForeignKey(sd => sd.TipDogadjajaID)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
