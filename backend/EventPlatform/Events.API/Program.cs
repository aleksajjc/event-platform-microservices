
using Events.API.Data;
using Events.API.HostedServices;

namespace Events.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSqlServer<EventContext>(builder.Configuration.GetConnectionString("DefaultConnectionEvent"));

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddHostedService<DogadjajOutboxPublisher>();
            
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
