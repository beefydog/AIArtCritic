using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ArtEvaluatorAPI.Models;
using ArtEvaluatorAPI.Services;
using ArtEvaluatorAPI.Middleware;
using Serilog;
using Microsoft.OpenApi.Models;
using ArtEvaluatorAPI.Configurations;
using ArtEvaluatorAPI.Controllers;

namespace ArtEvaluatorAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArtEvaluatorAPI", Version = "v1" });
            c.AddSecurityDefinition("userkey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "userkey",
                Type = SecuritySchemeType.ApiKey,
                Description = "Custom header for userkey"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "userkey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddSingleton<IChatPromptService, ChatPromptService>();
        builder.Services.AddSingleton<IDummyResponseService, DummyResponseService>();
        builder.Services.AddScoped<IChatController, ChatController>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder => builder.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArtEvaluatorAPI v1");
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseCors("AllowAllOrigins");

        app.UseMiddleware<UserKeyValidationMiddleware>();

        app.MapControllers();

        app.Run();
    }
}

