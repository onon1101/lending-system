namespace LendingSystem.WebApi.Options;

public interface IConfigurationOptions
{
    /// <summary>
    /// Binder name
    /// </summary>
    static abstract string SettingsName { get; }
}
