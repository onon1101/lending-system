using System.ComponentModel.DataAnnotations;
using LendingSystem.Auth.Domain.Enum;
using LendingSystem.Auth.Domain.ValueObjects;

namespace LendingSystem.Auth.Domain.Users;

// public sealed record User(
//     int Id,
//     string Email,
//     string PasswordHash,
//     string Name,
//     string Role,
//     string AuthProvider,
//     string? ProviderUserId,
//     DateTimeOffset CreatedAt,
//     DateTimeOffset UpdatedAt);

public sealed class UserEntity
{
    private UserEntity(int id, EmailEntity emailEntity, string passwordHash, string username, UserRole role, AuthProvider authProvider, string providerUserId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        EmailEntity = emailEntity;
        PasswordHash = passwordHash;
        Username = username;
        AuthProvider = authProvider;
        ProviderUserId = providerUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public int Id { get; init; }

    public EmailEntity EmailEntity { get; init; }
    
    public string PasswordHash { get; init; }
    
    public string Username { get; init; }
    
    public UserRole Role { get; init; }
    
    public AuthProvider AuthProvider { get; init; }
    
    public string ProviderUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }

    public static UserEntity Create(
        int id,
        string email,
        string passwordHash,
        string name,
        string role,
        string authProvider,
        string? providerUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        //todo: 看看能不能塞在 DI container 
        var emailEntity = EmailEntity.Create(new EmailAddressAttribute(), email);
        
        return new UserEntity(
            id, emailEntity, passwordHash, name,
            UserRoleExtensions.FromString(role),
            AuthProviderExtensions.FromString(authProvider),
            providerUserId ?? string.Empty, createdAt, updatedAt);
    }
}

public sealed record UserProfile(int UserId, string Name, string Email);
