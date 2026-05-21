using LendingSystem.Auth.Application.Auth;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Auth.Application.Abstractions;

public interface IGoogleOAuth2Acl
{
    Task<Result<ExternalLoginIdentity>> TranslateAsync(string idToken, CancellationToken cancellationToken);
}
