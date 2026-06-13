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

            
            builder.Services.AddSqlServer<PlacanjaContext>(builder.Configuration.GetConnectionString("DefaultConnectionPlacanja"));

            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            string sagaPattern = builder.Configuration["SagaPattern"] ?? "Orchestration";
            if (sagaPattern.Equals("Choreography", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddHostedService<SagaChoreographyConsumer>();
            }
            else
            {
                builder.Services.AddHostedService<SagaCommandConsumer>();
            }

            var app = builder.Build();

            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();

            
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PlacanjaContext>();
                db.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
