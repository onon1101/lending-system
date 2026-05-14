using LendingSystem.Application.Common;

namespace LendingSystem.Domain.Commons;

public abstract record Errors(
    string Code,
    string DevelopmentMessage,
    string PublicMessage,
    ErrorType Type)
{
    public string GetClientMessage(bool isDevelopment) =>
        isDevelopment ? DevelopmentMessage : PublicMessage;

    public static Errors None { get; } = new NoneErrors();
}

/// <summary>
/// 完全沒有錯誤
/// </summary>
public sealed record NoneErrors() 
    : Errors(
        string.Empty,
        string.Empty,
        string.Empty,
        ErrorType.None);

/// <summary>
/// 用於處理業務邏輯的錯誤
/// </summary>
/// <param name="Code"></param>
/// <param name="DevelopmentMessage"></param>
/// <param name="PublicMessage"></param>
/// <param name="ErrorType"></param>
public sealed record DomainErrors(
    string Code, 
    string DevelopmentMessage, 
    string PublicMessage)
    : Errors(Code, DevelopmentMessage, PublicMessage, ErrorType.Domain);

/// <summary>
/// 用於處理 Api 層的錯誤
/// </summary>
/// <param name="Code"></param>
/// <param name="DevelopmentMessage"></param>
/// <param name="PublicMessage"></param>
/// <param name="ErrorType"></param>
public sealed record ControllerErrors(
    string Code, 
    string DevelopmentMessage, 
    string PublicMessage, 
    ErrorType ErrorType)
    : Errors(Code, DevelopmentMessage, PublicMessage, ErrorType);
