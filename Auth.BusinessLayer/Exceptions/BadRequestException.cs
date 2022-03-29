namespace Auth.BusinessLayer.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) {}
}