using LendingSystem.WebApi.Startup;

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

namespace LendingSystem.WebApi
{
    public partial class Program;
}
