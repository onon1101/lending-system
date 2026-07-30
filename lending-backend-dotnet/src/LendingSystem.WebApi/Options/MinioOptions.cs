namespace LendingSystem.WebApi.Options;

public sealed class MinioOptions : IConfigurationOptions
{
    public static string SettingsName => "Minio";
    public string? Endpoint { get; set; } = null;
    public string? AccessKey { get; set; } = null;
    public string? SecretKey { get; set; } = null;
    public bool Ssl { get; set; } = true;
}
