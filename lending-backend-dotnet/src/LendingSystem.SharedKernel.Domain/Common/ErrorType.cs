namespace LendingSystem.SharedKernel.Domain.Common;

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
