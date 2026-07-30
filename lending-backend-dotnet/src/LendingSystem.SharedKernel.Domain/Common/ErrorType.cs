namespace LendingSystem.SharedKernel.Domain.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    BadGateway,
    ServiceUnavailable,
    ServerError
}
