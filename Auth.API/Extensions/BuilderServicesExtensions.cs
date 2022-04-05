using System.Text;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Auth.API.Extensions;

public static class BuilderServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IRequestHelper, RequestHelper>();
        services.AddScoped<IAuthProducer, AuthProducer>();
        services.AddScoped<IExceptionsHelper, ExceptionsHelper>();
        services.AddScoped<IInitializeMicroserviceModels, InitializeMicroserviceModels>();
        services.AddTransient<IInitializationService, InitializationService>();
    }

    public static void AddCustomAuth(this IServiceCollection services, string secretKey)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuerSigningKey = true
                    };
                });
        services.AddAuthorization();
    }

    public static void RegisterSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(config =>
        {
            config.EnableAnnotations();
            config.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "MyAPI",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Git Repository",
                        Url = new Uri("https://github.com/Stepa28/Auth")
                    }
                });
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public static void RegisterLogger(this IServiceCollection service, IConfiguration config)
    {
        service.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
        service.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            loggingBuilder.AddNLog(config);
        });
    }

    public static void AddMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CrmAddOrChangeLeadConsumer>();
            x.AddConsumer<AccountCheckingChangeRole>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ReceiveEndpoint("leadAddOrChangeAuthQueue", e =>
                {
                    e.PurgeOnStartup = true;
                    e.ConfigureConsumer<CrmAddOrChangeLeadConsumer>(context);
                });
                cfg.ReceiveEndpoint("leadChangeRoleQueue", e =>
                {
                    e.PurgeOnStartup = true;
                    e.ConfigureConsumer<AccountCheckingChangeRole>(context);
                });
            });
        });
    }

    public static async void InitializationLeads(this WebApplication app)
    {
        var timer = new Timer(3600000) { AutoReset = false };
        timer.Elapsed += async (sender, e) => await app.Services.CreateScope().ServiceProvider.GetRequiredService<IInitializationService>().InitializeMemoryCashAsync(timer);

        await app.Services.CreateScope().ServiceProvider.GetRequiredService<IInitializationService>().InitializeMemoryCashAsync(timer);
    }

    public static void InitializationConfiguration(this WebApplication app, string secretKey, string configAddress)
    {
        app.Configuration["secretKey"] = secretKey;
        app.Configuration[Microservice.MarvelousConfigs.ToString()] = configAddress;
    }
}