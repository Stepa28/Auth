using Auth.BusinessLayer.Configurations;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
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
    }

    public static void AddCustomAuth(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });
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
        var tmp = new InitializationService(new RequestHelper(),
            app.Services.GetRequiredService<ILogger<InitializationService>>(),
            app.Services.GetRequiredService<IMapper>(),
            app.Services.GetRequiredService<IMemoryCache>(),
            app.Services.CreateScope().ServiceProvider.GetRequiredService<IAuthProducer>(),
            app.Services.CreateScope().ServiceProvider.GetRequiredService<IAuthService>());

        var timer = new Timer(3600000) { AutoReset = false };
        timer.Elapsed += async (sender, e) => await tmp.InitializeMemoryCashAsync(timer);

        await tmp.InitializeMemoryCashAsync(timer);
    }
}