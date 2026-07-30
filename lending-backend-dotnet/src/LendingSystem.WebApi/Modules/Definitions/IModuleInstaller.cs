namespace LendingSystem.WebApi.Modules.Definitions;

/// <summary>
/// 第三方套件載入器的介面
/// </summary>
public interface IModuleInstaller
{
    /// <summary>
    /// The configuration of third-party
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment);

    /// <summary>
    /// How to startup
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    IApplicationBuilder UseApplication(WebApplication app);
}
