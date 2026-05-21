using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.Auth.Domain.Users;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class GoogleLoginCommandHandler(
    IUserRepository users,
    ITokenService tokens,
    IGoogleOAuth2Acl googleOAuth2) : IRequestHandler<GoogleLoginCommand, Result<GoogleLoginResult>>
{
    public async Task<Result<GoogleLoginResult>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Result<GoogleLoginResult>.Failure(AuthErrors.GoogleTokenRequired());
        }

        var externalIdentity = await googleOAuth2.TranslateAsync(request.IdToken, cancellationToken);
        if (!externalIdentity.IsSuccess)
        {
            return Result<GoogleLoginResult>.Failure(externalIdentity.Error);
        }

        var identity = externalIdentity.Data!;
        var user = await users.FindByProviderAsync(identity.Provider, identity.ProviderUserId, cancellationToken);
        if (user is not null)
        {
            var existingTokenPair = tokens.Generate(user);
            return Result<GoogleLoginResult>.Success(new GoogleLoginResult(existingTokenPair.AccessToken, existingTokenPair.RefreshToken));
        }

        user = await users.FindByEmailAsync(identity.Email, cancellationToken);
        if (user is null)
        {
            user = await users.CreateExternalAsync(
                identity.DisplayName,
                identity.Email,
                identity.Provider,
                identity.ProviderUserId,
                cancellationToken);
        }
        else if (user.AuthProvider != AuthProvider.Google || user.ProviderUserId != identity.ProviderUserId)
        {
            user = await users.LinkProviderAsync(user.Id, identity.Provider, identity.ProviderUserId, cancellationToken);
            if (user is null)
            {
                return Result<GoogleLoginResult>.Failure(AuthErrors.GoogleAccountLinkFailed());
            }
        }

        var tokenPair = tokens.Generate(user);
        return Result<GoogleLoginResult>.Success(new GoogleLoginResult(tokenPair.AccessToken, tokenPair.RefreshToken));
    }
}
