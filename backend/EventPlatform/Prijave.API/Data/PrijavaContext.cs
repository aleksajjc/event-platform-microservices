using Microsoft.EntityFrameworkCore;
using Prijave.API.Models;

namespace Prijave.API.Data
{
    public class PrijavaContext : DbContext
    {
        public PrijavaContext(DbContextOptions<PrijavaContext> options) : base(options)
        {
        }
        public DbSet<Ucesnik> Ucesnici { get; set; }
        public DbSet<Prijava> Prijave { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Prijava>()
                .HasKey(p => new { p.UcesnikID, p.StrucniDogadjajID });

            modelBuilder.Entity<Prijava>()
                .HasOne(p => p.Ucesnik)
                .WithMany(u => u.Prijave)
                .HasForeignKey(p => p.UcesnikID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
