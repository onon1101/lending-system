namespace LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;

public sealed class PasskeyOptions
{
    public const string SectionName = "Passkey";

    public string RelyingPartyId { get; init; } = "localhost";

    public string RelyingPartyName { get; init; } = "Lending System";

    public int Timeout { get; init; } = 60000;

    public string Attestation { get; init; } = "none";

    public PasskeyAuthenticatorSelectionOptions AuthenticatorSelection { get; init; } = new();

    public IReadOnlyCollection<PasskeyPublicKeyCredentialParameterOptions> PublicKeyCredentialParameters { get; init; } =
    [
        new()
    ];
}

public sealed class PasskeyAuthenticatorSelectionOptions
{
    public string ResidentKey { get; init; } = "preferred";

    public bool RequireResidentKey { get; init; }

    public string UserVerification { get; init; } = "preferred";
}

public sealed class PasskeyPublicKeyCredentialParameterOptions
{
    public string Type { get; init; } = "public-key";

    public int Algorithm { get; init; } = -7;
}
