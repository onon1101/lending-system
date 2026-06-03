using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;

public sealed record PasskeyRegistrationOptionResult(
    [property: JsonPropertyName("rp")]
    SystemInfo SystemInfo,
    
    [property: JsonPropertyName("user")]
    UserInfo UserInfo,

    [property: JsonPropertyName("challenge")]
    string Challenge,

    [property: JsonPropertyName("pubKeyCredParams")]
    IReadOnlyCollection<PublicKeyCredentialParameter> PublicKeyCredentialParameters,

    [property: JsonPropertyName("timeout")]
    int Timeout,

    [property: JsonPropertyName("excludeCredentials")]
    IReadOnlyCollection<PublicKeyCredentialDescriptor> ExcludeCredentials,

    [property: JsonPropertyName("authenticatorSelection")]
    AuthenticatorSelectionCriteria AuthenticatorSelection,

    [property: JsonPropertyName("attestation")]
    string Attestation);

public sealed record SystemInfo(
    [property: JsonPropertyName("id")]
    string Id, 
    
    [property: JsonPropertyName("name")]
    string Name);
    
public sealed record UserInfo(
    [property: JsonPropertyName("id")]
    string UserId,
    
    [property: JsonPropertyName("name")]
    string Username, 

    [property: JsonPropertyName("displayName")]
    string DisplayName,
    
    [property: JsonPropertyName("email")]
    string Email);

public sealed record PublicKeyCredentialParameter(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("alg")]
    int Algorithm);

public sealed record PublicKeyCredentialDescriptor(
    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("id")]
    string Id,

    [property: JsonPropertyName("transports")]
    IReadOnlyCollection<string> Transports);

public sealed record AuthenticatorSelectionCriteria(
    [property: JsonPropertyName("residentKey")]
    string ResidentKey,

    [property: JsonPropertyName("requireResidentKey")]
    bool RequireResidentKey,

    [property: JsonPropertyName("userVerification")]
    string UserVerification);
