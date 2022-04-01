namespace Auth.BusinessLayer.Producers;

public interface IAuthProducer
{
    Task NotifyFatalError(string message);
}