using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;

namespace Auth.BusinessLayer.Helpers;

public interface IExceptionsHelper
{
    /// <summary>
    ///     Проверяет lead на значение по умолчанию
    /// </summary>
    /// <param name="lead">то что проверяют</param>
    /// <param name="email">email для логирования исключения</param>
    /// <exception cref="NotFoundException">Структура lead имеет значение по умолчанию</exception>
    void ThrowIfEmailNotFound(string email, LeadAuthModel lead);

    /// <summary>
    ///     Проверяет пароль на соответствие хешу
    /// </summary>
    /// <param name="pass">пароль</param>
    /// <param name="hashPassFromBd">хеш пароля</param>
    /// <exception cref="IncorrectPasswordException">Пароль не совпадает с хешем</exception>
    void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd);
}