namespace Auth.BusinessLayer.Producers;

public interface IAuthProducer
{
    Task NotifyErrorByEmail(string message);
}