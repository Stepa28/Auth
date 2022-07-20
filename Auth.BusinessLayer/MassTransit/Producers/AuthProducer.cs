using Auth.Resources;
using Marvelous.Contracts.EmailMessageModels;
using Marvelous.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Producers;

public class AuthProducer : IAuthProducer
{
    private readonly IBus _bus;
    private readonly ILogger<AuthProducer> _logger;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public AuthProducer(IBus bus, ILogger<AuthProducer> logger, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _bus = bus;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task NotifyErrorByEmail(string message)
    {
        var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _logger.LogInformation(_localizer["ReportFailedInitialization"]);

        await _bus.Publish(new EmailErrorMessage
            {
                ServiceName = Microservice.MarvelousAuth.ToString(),
                TextMessage = message
            },
            source.Token);

        _logger.LogInformation(_localizer["AlertSent"]);
    }
}