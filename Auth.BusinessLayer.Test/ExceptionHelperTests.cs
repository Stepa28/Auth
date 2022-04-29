using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class ExceptionHelperTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private IExceptionsHelper _helper;
    private Mock<ILogger<ExceptionsHelper>> _logger;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<ExceptionsHelper>>();
        _helper = new ExceptionsHelper(_logger.Object);
    }

    #endregion

    #region ExceptionHelper

    [Test]
    public void ThrowIfEmailNotFound_ValidRequestReceived_ShouldNothing()
    {
        //given
        var email = "test@example.com";
        var lead = new LeadAuthModel { HashPassword = "1000", Id = default, Role = default };

        //when
        _helper.ThrowIfEmailNotFound(email, lead);
    }

    [Test]
    public void ThrowIfEmailNotFound_WhenLeadDefault_ShouldThrowNotFoundException()
    {
        //given
        var email = "test@example.com";
        var lead = new LeadAuthModel();
        var expected = $"Entity with e-mail = {email} not found";

        //when
        var actual = Assert.Throws<NotFoundException>(() => _helper.ThrowIfEmailNotFound(email, lead))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void ThrowIfPasswordIsIncorrected_ValidRequestReceived_ShouldNothing()
    {
        //given
        var password = "test";
        var hashPassword = "1000:TSv9KT93D9MktmGF2dy0TBcNjXqpy5n1:DTpwmnIeUP+QikKqyvQEfY1RgAs=";

        //when
        _helper.ThrowIfPasswordIsIncorrected(password, hashPassword);
    }

    [Test]
    public void ThrowIfPasswordIsIncorrected_WhenIncorrectedPassword_ShouldThrowIncorrectPasswordException()
    {
        //given
        var password = "test";
        var hashPassword = "1000:TSv9KT93D9MktmGF2dy0TBcNjXqpy5n1:DTpemnIeUP+QikKqyvQEfY1RgAs=";
        var expected = "Incorrected password";

        //when
        var actual = Assert.Throws<IncorrectPasswordException>(() => _helper.ThrowIfPasswordIsIncorrected(password, hashPassword))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

}