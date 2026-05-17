namespace LendingSystem.Domain.Commons;

public enum ErrorType
{
    None,
    Domain,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    BadGateway,
    ServiceUnavailable,
    ServerError
}
