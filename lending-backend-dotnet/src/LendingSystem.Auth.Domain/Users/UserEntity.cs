using System.ComponentModel.DataAnnotations;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

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

public sealed class UserEntity : Entity, IAggregateRoot
{
    private UserEntity(long id, Email email, string passwordHash, string username, UserRole role, AuthProvider authProvider, string providerUserId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Username = username;
        Role = role;
        AuthProvider = authProvider;
        ProviderUserId = providerUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public long Id { get; init; }

    public Email Email { get; init; }
    
    public string PasswordHash { get; init; }
    
    public string Username { get; init; }
    
    public UserRole Role { get; init; }
    
    public AuthProvider AuthProvider { get; init; }
    
    public string ProviderUserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }

    public static UserEntity Create(
        EmailAddressAttribute emailAddressAttribute,
        long id,
        string email,
        string passwordHash,
        string name,
        string role,
        string authProvider,
        string? providerUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        var emailEntity = Email.Create(emailAddressAttribute, email);
        
        return new UserEntity(
            id, emailEntity, passwordHash, name,
            UserRole.FromString(role),
            AuthProvider.FromString(authProvider),
            providerUserId ?? string.Empty, createdAt, updatedAt);
    }
}

public sealed record UserProfile(long UserId, string Name, string Email);
