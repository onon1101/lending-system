using Google.Apis.Auth;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.Auth.Domain.Users;
using LendingSystem.SharedKernel.Application.Common;
using Microsoft.Extensions.Configuration;

namespace LendingSystem.Auth.ACL.Google;

public sealed class GoogleOAuth2Acl(IConfiguration configuration) : IGoogleOAuth2Acl
{
    public async Task<Result<ExternalLoginIdentity>> TranslateAsync(string idToken, CancellationToken cancellationToken)
    {
        var googleClientId = configuration["GOOGLE_CLIENT_ID"] ?? configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(googleClientId))
        {
            return Result<ExternalLoginIdentity>.Failure(AuthErrors.GoogleClientIdNotConfigured());
        }

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [googleClientId]
                });
        }
        catch (InvalidJwtException)
        {
            return Result<ExternalLoginIdentity>.Failure(AuthErrors.GoogleLoginFailed());
        }

        if (payload.EmailVerified != true ||
            string.IsNullOrWhiteSpace(payload.Email) ||
            string.IsNullOrWhiteSpace(payload.Subject))
        {
            return Result<ExternalLoginIdentity>.Failure(AuthErrors.GoogleAccountNotVerified());
        }

        var displayName = payload.Name ?? payload.Email.Split('@')[0];
        return Result<ExternalLoginIdentity>.Success(
            new ExternalLoginIdentity(
                AuthProvider.Google,
                payload.Subject,
                payload.Email,
                displayName));
    }
}
