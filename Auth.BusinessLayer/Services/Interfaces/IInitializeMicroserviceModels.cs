using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Services;

public interface IInitializeMicroserviceModels
{
    Dictionary<Microservice, MicroserviceModel> InitializeMicroservices();
}