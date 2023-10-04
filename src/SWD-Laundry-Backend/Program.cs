using System.Text.RegularExpressions;
using Invedia.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SWD_Laundry_Backend.Contract.Repository.Entity.IdentityModels;
using SWD_Laundry_Backend.Core.Config;
using SWD_Laundry_Backend.Extensions;
using SWD_Laundry_Backend.Repository.Infrastructure;

namespace SWD_Laundry_Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Environment.EnvironmentName = Environments.Development;
        //builder.Environment.EnvironmentName = Environments.Production;

        // Add services to the container.

        SystemSettingModel.Configs = builder.Configuration.AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", false, true)
                .AddUserSecrets<Program>(true, false)
                .Build();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


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
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "SWD-Laundry-Backend", Version = "v1" });
        });
        builder.Services.AddCors();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Jwt:Authority"];
            options.Audience = builder.Configuration["Jwt:Audience"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience =  builder.Configuration["Jwt:Audience"],
                ValidIssuer = builder.Configuration["Jwt:Authority"],
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Staff", policy => policy.RequireRole("Staff"));
            options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
            options.AddPolicy("LaundryStore", policy => policy.RequireRole("LaundryStore"));
        });
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                );
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        });
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddAutoMapperServices();

        builder.Services.AddRouting(options =>
        {
            options.AppendTrailingSlash = false;
        });
        _ = builder.Services.AddSystemSetting(builder.Configuration.GetSection("SystemSetting").Get<SystemSettingModel>());
        builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromMinutes(30));
        builder.Services.AddDI();
        builder.Services.PrintServiceAddedToConsole();
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
        app.UseAuthentication();

        //app.UseIdentityServer();
        app.UseAuthorization();
        app.MapControllers();
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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

public static class StartupSystemSetting
{
    public static IServiceCollection AddSystemSetting(this IServiceCollection services, SystemSettingModel? systemSettingModel)
    {
        SystemSettingModel.Instance = systemSettingModel ?? new SystemSettingModel();

        return services;
    }
}
