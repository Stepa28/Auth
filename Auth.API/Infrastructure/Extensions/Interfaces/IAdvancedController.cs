using System.Security.Authentication;
using Auth.BusinessLayer.Exceptions;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Extensions;

public interface IAdvancedController
{
    /// <summary>
    ///     Возвращает микросервис который вызывает Endpoint
    /// </summary>
    /// <exception cref="ForbiddenException">Попытка обращения с не зарегистрированного IP или в Heads запроса нет нужных данных </exception>
    Microservice Service { get; }

    /// <summary>
    ///     Возвращает слушателей токена
    /// </summary>
    /// <exception cref="AuthenticationException">Не удалось получить необходимые данные их токена</exception>
    string Audience { get; }

    /// <summary>
    ///     Возвращает издателя токена
    /// </summary>
    /// <exception cref="AuthenticationException">Не удалось получить необходимые данные их токена</exception>
    string Issuer { get; }

    /// <summary>
    ///     Возвращает модель индитификатора из токена
    /// </summary>
    /// <exception cref="AuthenticationException">Не удалось получить необходимые данные их токена</exception>
    IdentityResponseModel Identity { get; }

    /// <summary>
    ///     Устанавливает экземпляр контройлера из которого получаются данные о токене 
    /// </summary>
    Controller Controller { set; }
}