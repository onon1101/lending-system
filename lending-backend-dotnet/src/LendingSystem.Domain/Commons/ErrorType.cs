namespace LendingSystem.Application.Common;

public enum ErrorType
{
    None,
    Domain,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    BadGateway,
    ServerError
}