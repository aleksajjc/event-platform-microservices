
using Microsoft.Extensions.Logging;
using Prijave.API.Background_services;
using Prijave.API.Data;
using Prijave.API.HostedServices;

namespace Prijave.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSqlServer<PrijavaContext>(builder.Configuration.GetConnectionString("DefaultConnectionPrijava"));
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<DeleteConsumer>();
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
            builder.Services.AddHostedService<DogadjajCreatedConsumer>();
            builder.Services.AddSingleton<DogadjajDetaljiClient>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<DogadjajDetaljiClient>());
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
