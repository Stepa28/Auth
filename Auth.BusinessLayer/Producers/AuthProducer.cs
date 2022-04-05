using Marvelous.Contracts.EmailMessageModels;
using Marvelous.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Producers;

public class AuthProducer : IAuthProducer
{
    private readonly IBus _bus;
    private readonly ILogger<AuthProducer> _logger;
    
    public AuthProducer(IBus bus, ILogger<AuthProducer> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task NotifyFatalError(string message)
    {
        var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _logger.LogInformation("Attempt to report failed initialization");

        await _bus.Publish<EmailErrorMessage>(new
        {
            ServiceName = Microservice.MarvelousAuth.ToString(),
            TextMessage = message
        }, source.Token);
        
        _logger.LogInformation("Initialization failure alert sent");
    }
}