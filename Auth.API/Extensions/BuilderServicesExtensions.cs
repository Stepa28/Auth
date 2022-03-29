﻿using Auth.BusinessLayer.Services;
using MassTransit;
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
            //x.AddConsumer<LeadConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost",
                    hst =>
                    {
                        hst.Username("nafanya");
                        hst.Password("qwe!23");
                    });
                /*cfg.ReceiveEndpoint("leadCRMQueue", e =>
                {
                    e.ConfigureConsumer<LeadConsumer>(context);
                });*/
            });
        });
    }
}