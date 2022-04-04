using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Models;

public class MicroserviceModel
{
    private readonly Func<string> _servicesThatHaveAccess;

    public MicroserviceModel(string ip, Func<string> servicesThatHaveAccess, Microservice microservice)
    {
        Ip = ip;
        _servicesThatHaveAccess = servicesThatHaveAccess;
        Microservice = microservice;
    }

    public string Ip { get; }
    public Microservice Microservice { get; }
    public string GetServicesThatHaveAccess() => _servicesThatHaveAccess();
    public Frontend Frontend { get; set; }
}