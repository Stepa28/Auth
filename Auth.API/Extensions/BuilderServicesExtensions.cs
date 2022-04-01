using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using MassTransit;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;

namespace Auth.API.Extensions;

public static class BuilderServicesExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<IRequestHelper, RequestHelper>();
        services.AddScoped<IAuthProducer, AuthProducer>();
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
}