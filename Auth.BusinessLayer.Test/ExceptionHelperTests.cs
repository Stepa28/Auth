using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class ExceptionHelperTests
{
    private readonly Mock<ILogger<ExceptionsHelper>> _logger;
    private IExceptionsHelper _helper;

    public ExceptionHelperTests()
    {
        _logger = new Mock<ILogger<ExceptionsHelper>>();
    }

    [SetUp]
    public void SetUp()
    {
        _helper = new ExceptionsHelper(_logger.Object);
    }

    [Test]
    public void ThrowIfEmailNotFoundTest()
    {
        //given
        var email = "test@example.com";
        var lead = new LeadAuthModel { HashPassword = "1000", Id = default, Role = default };

        //when
        _helper.ThrowIfEmailNotFound(email, lead);
    }

    [Test]
    public void ThrowIfEmailNotFoundNegativeTest_NotFoundException()
    {
        //given
        var email = "test@example.com";
        var lead = new LeadAuthModel { HashPassword = default, Id = default, Role = default };
        var expected = $"Entity with e-mail = {email} not found";

        //when
        var actual = Assert.Throws<NotFoundException>(() => _helper.ThrowIfEmailNotFound(email, lead))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    public void ThrowIfPasswordIsIncorrectedTest()
    {
        //given
        var password = "test";
        var hashPassword = "1000:TSv9KT93D9MktmGF2dy0TBcNjXqpy5n1:DTpwmnIeUP+QikKqyvQEfY1RgAs=";

        //when
        _helper.ThrowIfPasswordIsIncorrected(password, hashPassword);
    }

    [Test]
    public void ThrowIfPasswordIsIncorrectedNegativeTest_IncorrectPasswordException()
    {
        //given
        var password = "test";
        var hashPassword = "1000:TSv9KT93D9MktmGF2dy0TBcNjXqpy5n1:DTpemnIeUP+QikKqyvQEfY1RgAs=";
        var expected = "Incorrected password";

        //when
        var actual = Assert.Throws<IncorrectPasswordException>(() => _helper.ThrowIfPasswordIsIncorrected(password, hashPassword))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }
}