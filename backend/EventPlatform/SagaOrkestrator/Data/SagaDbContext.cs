using Microsoft.EntityFrameworkCore;
using SagaOrkestrator.Entities;

namespace SagaOrkestrator.Data
{
    public class SagaDbContext : DbContext
    {
        public SagaDbContext()
        {
        }

        public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
        {
        }

        public DbSet<SagaState> SagaStates { get; set; }
        public DbSet<SagaCommandOutboxMessage> SagaCommandOutboxMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Koristi LocalDB bazu podataka za Saga Orkestrator
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SagaOrkestratorDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            }
        }
    }
}
