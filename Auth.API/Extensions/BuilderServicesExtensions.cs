using System.Text;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;

namespace Auth.API.Extensions;

public static class BuilderServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuthService, AuthService>();

        services.AddSingleton<IRequestHelper, RequestHelper>();
        services.AddSingleton<IAuthProducer, AuthProducer>();
        services.AddSingleton<IExceptionsHelper, ExceptionsHelper>();
        services.AddTransient<IInitializationLeads, InitializationLeads>();
        services.AddTransient<IInitializationConfigs, InitializationConfigs>();
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
            config.AddSecurityDefinition("Bearer",
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
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
            loggingBuilder.AddNLog(config);
        });
    }

    public static void AddMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<CrmAddOrChangeLeadConsumer>();
            x.AddConsumer<AccountCheckingChangeRoleConsumer>();
            x.AddConsumer<ConfigChangeConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ReceiveEndpoint("leadAddOrChangeAuthQueue",
                    e =>
                    {
                        e.PurgeOnStartup = true;
                        e.ConfigureConsumer<CrmAddOrChangeLeadConsumer>(context);
                    });
                cfg.ReceiveEndpoint("leadChangeRoleQueue",
                    e =>
                    {
                        e.PurgeOnStartup = true;
                        e.ConfigureConsumer<AccountCheckingChangeRoleConsumer>(context);
                    });
                cfg.ReceiveEndpoint("ChangeConfigAuth",
                    e =>
                    {
                        e.PurgeOnStartup = true;
                        e.ConfigureConsumer<ConfigChangeConsumer>(context);
                    });
            });
        });
    }
}