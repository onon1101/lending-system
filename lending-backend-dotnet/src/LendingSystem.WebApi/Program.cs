using LendingSystem.WebApi.Startup;
namespace LendingSystem.WebApi;

/// <summary>
/// Program Class
/// </summary>
public sealed class Program
{

    /// <summary>
    /// Program Entry Point
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Generate the builder
        var builder = WebApplication.CreateBuilder(args);

        // config the third party settings
        builder.ConfigureWebHost();
        builder.Services.AddAllModules(
            builder.Configuration,
            builder.Environment,
            typeof(Program).Assembly);

        // startup the service 
        var app = builder.Build();

        app.UseAllModules();

        // finally, run it
        app.Run();
    }
}
