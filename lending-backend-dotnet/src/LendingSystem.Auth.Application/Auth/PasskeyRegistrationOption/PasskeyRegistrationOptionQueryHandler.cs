using System.Security.Cryptography;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;

internal sealed class PasskeyRegistrationOptionQueryHandler(
    IExecutionContextAccessor executionContextAccessor,
    IUserQueryRepository userQueryRepository,
    IOptions<PasskeyOptions> passkeyOptions)
: IRequestHandler<PasskeyRegistrationOptionQuery, Result<PasskeyRegistrationOptionResult>>
{
    public async Task<Result<PasskeyRegistrationOptionResult>> Handle(
        PasskeyRegistrationOptionQuery request, 
        CancellationToken cancellationToken)
    {
        var userId = executionContextAccessor.Current.User.UserId;
        var user = await userQueryRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<PasskeyRegistrationOptionResult>.Failure(AuthErrors.UserNotFound());
        }

        var options = passkeyOptions.Value;
        var result = new PasskeyRegistrationOptionResult(
            new SystemInfo(options.RelyingPartyId, options.RelyingPartyName),
            new UserInfo(
                Base64UrlEncode(BitConverter.GetBytes(user.UserId)),
                user.Name,
                user.Name,
                user.Email),
            GenerateChallenge(),
            options.PublicKeyCredentialParameters
                .Select(parameter => new PublicKeyCredentialParameter(parameter.Type, parameter.Algorithm))
                .ToArray(),
            options.Timeout,
            [],
            new AuthenticatorSelectionCriteria(
                options.AuthenticatorSelection.ResidentKey,
                options.AuthenticatorSelection.RequireResidentKey,
                options.AuthenticatorSelection.UserVerification),
            options.Attestation);

        return Result<PasskeyRegistrationOptionResult>.Success(result);
    }

    private static string GenerateChallenge()
    {
        Span<byte> challenge = stackalloc byte[32];
        RandomNumberGenerator.Fill(challenge);
        return Base64UrlEncode(challenge);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
