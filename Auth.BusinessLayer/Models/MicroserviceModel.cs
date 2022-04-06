using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Models;

public class MicroserviceModel
{
    public MicroserviceModel(string? address, string servicesThatHaveAccess, Microservice microservice, Microservice frontend)
    {
        Address = address ?? "";
        ServicesThatHaveAccess = servicesThatHaveAccess;
        Microservice = microservice;
        Frontend = frontend;
    }

    public string Address { get; }
    public Microservice Microservice { get; }
    public string ServicesThatHaveAccess { get; }
    public Microservice Frontend { get; set; }
}