using System.Security.Authentication;
using Auth.BusinessLayer.Exceptions;
using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Services;

public interface IAuthService
{
    /// <summary>
    ///     Выдаёт токен по email и паролю, если данный lead есть в памяти микросервиса
    /// </summary>
    /// <param name="email">Email lead по которому производится попытка аутентификации</param>
    /// <param name="pass">Пароль lead по которому производится попытка аутентификации</param>
    /// <param name="service">Microservice который запросил токен</param>
    /// <exception cref="ServiceUnavailableException">Инициализация leads закончилась с ошибкой</exception>
    /// <exception cref="NotFoundException">Lead с данным email не найден в памяти</exception>
    /// <exception cref="IncorrectPasswordException">Пароль lead не совпадает с хранящемся в памяти хешем</exception>
    string GetTokenForFront(string email, string pass, Microservice service);

    /// <summary>
    ///     Выдаёт токен микросервису
    /// </summary>
    /// <param name="service">Microservice который запросил токен</param>
    string GetTokenForMicroservice(Microservice service);

    /// <summary>
    ///     Проверяет есть ли доступ у издавшего токен микросервиса к запросившему проверку
    /// </summary>
    /// <param name="issuerToken">Микросервис издавшей токен</param>
    /// <param name="audienceToken">Список микросервисов к которым издавшей имеет доступ (из токена)</param>
    /// <param name="service">Микросервис который запросил проверку токена</param>
    /// <exception cref="AuthenticationException">Данные из токена и модели микросервисов не совпали</exception>
    /// <exception cref="ForbiddenException">У микросервиса издавшего токен нет доступа к запрашивающего проверку</exception>
    bool CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service);

    /// <summary>
    ///     Проверяет есть ли доступ у front издавшего токен микросервиса
    /// </summary>
    /// <param name="issuerToken">Микросервис издавшей токен</param>
    /// <param name="audienceToken">Список микросервисов к которым издавшей имеет доступ (из токена)</param>
    /// <param name="service">Microservice который запросил проверку токена</param>
    /// <exception cref="AuthenticationException">Микросервис запросившей проверку токена не издавал его</exception>
    /// <exception cref="ForbiddenException">Front издавшего токен микросервиса не имеет доступ</exception>
    bool CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service);

    /// <summary>
    ///     Выполняет двойную проверку
    /// </summary>
    /// <param name="issuerToken">Микросервис издавшей токен</param>
    /// <param name="audienceToken">Список микросервисов к которым издавшей имеет доступ (из токена)</param>
    /// <param name="service">Microservice который запросил проверку токена</param>
    /// <exception cref="AuthenticationException">
    ///     Микросервис запросившей проверку токена не издавал его или данные из токена и
    ///     модели микросервисов не совпали
    /// </exception>
    /// <exception cref="ForbiddenException">
    ///     Front издавшего токен микросервиса не имеет доступ или у микросервиса издавшего
    ///     токен нет доступа к запрашивающего проверку
    /// </exception>
    bool CheckDoubleValidToken(string issuerToken, string audienceToken, Microservice service);

    /// <summary>
    ///     Хеширует пароль
    /// </summary>
    /// <param name="password">Пароль</param>
    string GetHashPassword(string password);
}