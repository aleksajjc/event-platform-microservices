
using EventPlatformGateway.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using System.Text;
using Ocelot.Cache.CacheManager;
using Ocelot.Middleware;
namespace EventPlatformGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Configuration
                    .SetBasePath(builder.Environment.ContentRootPath)
                    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.RequireHttpsMetadata = true;

                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                                    ValidateAudience = true,
                                    ValidAudience = builder.Configuration["Jwt:Audience"],

                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

                                    ValidateLifetime = true,
                                    ClockSkew = TimeSpan.Zero
                                };
                            });

            builder.Services
                .AddOcelot(builder.Configuration)
                .AddCacheManager(x => x.WithDictionaryHandle());

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<RequestSecurityMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            var pipeline = new OcelotPipelineConfiguration
            {
                AuthorizationMiddleware = async (context, next) =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Custom Authorization Middleware se izvrsava");
                    await next.Invoke();
                }
            };

            await app.UseOcelot(pipeline);

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
