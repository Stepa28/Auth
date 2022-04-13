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
    public Microservice Frontend { get; }

    private bool Equals(MicroserviceModel other)
    {
        return Microservice == other.Microservice && ServicesThatHaveAccess == other.ServicesThatHaveAccess && Frontend == other.Frontend;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj.GetType() == GetType() && Equals((MicroserviceModel)obj);
    }
}