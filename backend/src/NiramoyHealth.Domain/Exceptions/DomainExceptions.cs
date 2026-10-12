namespace NiramoyHealth.Domain.Exceptions;

public abstract class NiramoyException : Exception
{
    protected NiramoyException(string message) : base(message) { }
    protected NiramoyException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotFoundException : NiramoyException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.") { }
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : NiramoyException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}

public class UnauthorizedException : NiramoyException
{
    public UnauthorizedException(string message = "Authentication failed or token is invalid.")
        : base(message) { }
}

public class ForbiddenException : NiramoyException
{
    public ForbiddenException(string message = "You do not have permission to access this resource.")
        : base(message) { }
}

public class ConflictException : NiramoyException
{
    public ConflictException(string message) : base(message) { }
}

public class BusinessRuleViolationException : NiramoyException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}
