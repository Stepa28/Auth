using Timer = System.Timers.Timer;

namespace Auth.BusinessLayer.Services;

public interface IInitializationLeads
{
    Task InitializeMemoryCashAsync(Timer timer);
}