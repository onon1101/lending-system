namespace LendingSystem.WebApi.Options;

public sealed class DatabaseOptions : IConfigurationOptions
{
    public static string SettingsName => "Database";
    public string? Host { get; set; } = null;
    public int Port { get; set; } = 5432;
    public string? User { get; set; } = null;
    public string? Password { get; set; } = null;
    public string? Name { get; set; } = null;
    public string? SslMode { get; set; } = null;
    public bool Pooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 25;
}
