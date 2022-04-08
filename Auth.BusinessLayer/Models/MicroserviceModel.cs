using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Models;

public class MicroserviceModel
{
    public MicroserviceModel(string servicesThatHaveAccess, Microservice microservice, Microservice frontend)
    {
        ServicesThatHaveAccess = servicesThatHaveAccess;
        Microservice = microservice;
        Frontend = frontend;
    }

    public Microservice Microservice { get; }
    public string ServicesThatHaveAccess { get; }
    public Microservice Frontend { get; set; }
}