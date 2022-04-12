namespace Auth.BusinessLayer.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) {}
}