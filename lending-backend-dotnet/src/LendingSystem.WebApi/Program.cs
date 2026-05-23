using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;
using LendingSystem.SharedKernel.Domain.Common;
using LendingSystem.Infrastructure;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using LendingSystem.WebApi.Controllers;
using LendingSystem.WebApi.Configuration.Authorization;
using LendingSystem.WebApi.Configuration.ExecutionContext;
using LendingSystem.WebApi.Middleware;
using LendingSystem.WebApi.Models;
using LendingSystem.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

var appPort = builder.Configuration["APP_PORT"] ?? builder.Configuration["App:Port"] ?? "8000";
builder.WebHost.UseUrls($"http://0.0.0.0:{appPort}");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join("; ", context.ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid request body" : x.ErrorMessage));

        return new BadRequestObjectResult(ToFailureResponse(ControllerApiErrors.InvalidRequestBody(message), isDevelopment));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "物品借閱系統 API", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();

var secretKey = builder.Configuration["SECRET_KEY"] ?? builder.Configuration["Jwt:SecretKey"] ?? "development-secret-key-change-before-production";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ToFailureResponse(ControllerApiErrors.Unauthorized(), isDevelopment));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ToFailureResponse(ControllerApiErrors.Forbidden(), isDevelopment));
            }
        };
    });

builder.Services.AddPermissionAuthorization();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "物品借閱系統 API v1"));
}

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static ApiResponse<object> ToFailureResponse(Errors error, bool isDevelopment) =>
    ApiResponse<object>.Failure(error.Code, error.GetClientMessage(isDevelopment));
