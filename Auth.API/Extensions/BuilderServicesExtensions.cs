using Auth.API.Consumer;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;

namespace Auth.API.Extensions;

public static class BuilderServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
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

            /*config.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
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
                    new string[] {}
                }
            });*/
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
            x.AddConsumer<CrmAddLeadOrChangePasswordConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost",
                    hst =>
                    {
                        hst.Username("nafanya");
                        hst.Password("qwe!23");
                    });
                cfg.ReceiveEndpoint("leadAddAuthQueue", e =>
                {
                    e.PurgeOnStartup = true;
                    e.ConfigureConsumer<CrmAddLeadOrChangePasswordConsumer>(context);
                });
            });
        });
    }

    public static void InitializationMamoryCash(this IMemoryCache memoryCache)
    {
        //TODO проработать логику инициализации
        memoryCache.Set("321@example.com",
            new LeadAuthModel { Id = 1, HashPassword = "1000:Sh979Zdl5gKkAXdniuIV3ZCkZXvL94Vk:GwKwxRlEMwdEIvEHLKxiV03s+W8=", Role = Role.Admin });
    }
}