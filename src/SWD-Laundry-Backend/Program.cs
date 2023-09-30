using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using Serilog;
using SWD_Laundry_Backend.Extensions;

namespace SWD_Laundry_Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Environment.EnvironmentName = Environments.Development;
        // Add services to the container.

        builder.Configuration.AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", false, true)
                .AddUserSecrets<Program>(true, false)
                .Build();

        builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                   .ReadFrom.Configuration(hostingContext.Configuration)
                              .Enrich.FromLogContext()
                                         .WriteTo.Console());
        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen( options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "SWD-Laundry-Backend", Version = "v1" });
        });

        builder.Services.AddAutoMapperServices();
        builder.Services.AddRouting(options =>
        {
            options.AppendTrailingSlash = true;
        }  );

        builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromMinutes(30));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SWD-Laundry-Backend v1");
                    options.RoutePrefix = "swagger/api/v1";
                });
        }

        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object value)
    {
        // Slugify value
        return value == null ? null : Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
