using Timer = System.Timers.Timer;

namespace Auth.BusinessLayer.Services;

public interface IInitializationService
{
    Task InitializeMemoryCashAsync(Timer timer);
}