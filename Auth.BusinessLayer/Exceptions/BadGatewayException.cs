namespace Auth.BusinessLayer.Exceptions;

public class BadGatewayException : Exception
{
    public BadGatewayException(string message) : base(message) {}
}