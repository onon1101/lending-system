namespace LendingSystem.WebApi.Options;

public sealed class AppOptions : IConfigurationOptions
{
    public static string SettingsName => "App";

    public int? Port { get; set; } = 8080;

    public string? Ip { get; set; } = "0.0.0.0";
}
