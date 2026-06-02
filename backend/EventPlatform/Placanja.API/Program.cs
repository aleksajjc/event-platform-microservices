using Microsoft.EntityFrameworkCore;
using Placanja.API.Data;
using Placanja.API.HostedServices;

namespace Placanja.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Dodajemo SQL Server bazu podataka za plaćanje
            builder.Services.AddSqlServer<PlacanjaContext>(builder.Configuration.GetConnectionString("DefaultConnectionPlacanja"));

            // Dodajemo standardne API servise
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Registrujemo Saga potrošač komandi za plaćanje kao pozadinski servis
            builder.Services.AddHostedService<SagaCommandConsumer>();

            var app = builder.Build();

            // Konfiguracija HTTP zahteva za razvojno okruženje
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            // Osiguravamo da baza i seedovani podaci za plaćanja postoje na mašini
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PlacanjaContext>();
                db.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
