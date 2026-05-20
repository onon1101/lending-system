using System.ComponentModel.DataAnnotations;

namespace LendingSystem.Auth.Domain.ValueObjects;

public sealed class EmailEntity
{
    private EmailEntity(string localPart, string domainPart)
    {
        this.LocalPart = localPart;
        this.DomainPart = domainPart;
    }

    private string LocalPart { get; init; }

    private string DomainPart { get; init; }

    public static EmailEntity Create(
        EmailAddressAttribute emailAddressAttribute,
        string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new  ArgumentException("Email 不可為空");

        if (!emailAddressAttribute.IsValid(email))
            throw new ArgumentException("無效的 email 格式");
        
        var parts = email.Split('@');
        if (parts.Length != 2)
            throw new ArgumentException("無效的 Email 格式");

        var domainParts = parts[1].Split('.');

        if (!(domainParts.Length >= 2 &&
              domainParts.All(p => !string.IsNullOrWhiteSpace(p)) &&
              domainParts[^1].Length >= 2))
            throw new ArgumentException("無效的 Email 格式");

        return new EmailEntity(parts[0], parts[1]);
    }

    public string GetEmailStr()
    {
        return $"{LocalPart}@{DomainPart}";
    }
}
