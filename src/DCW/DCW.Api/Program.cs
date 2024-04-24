using System.Text.Json.Serialization;
using DCW.Api.Authentication;
using DCW.Api.Options;
using DCW.Interfaces;
using DCW.Services.Azure;
using DCW.Shared;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.StorageOptionsSettingsName))
    .ValidateDataAnnotations();
builder.Services.AddOptions<AzureAdOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AzureAdSettingsName));
builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AuthOptionsSectionName))
    .ValidateDataAnnotations();
builder.Services.AddOptions<AzureMonOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AzureMonSettingsName))
    .ValidateDataAnnotations();
builder.Services.AddHealthChecks();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(conf =>
{
    conf.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Api key to access the CA api's",
        Type = SecuritySchemeType.ApiKey,
        Name = AuthOptions.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };
    conf.AddSecurityRequirement(requirement);
});

builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddCors();

var storageOptions = builder.Configuration.GetSection(SettingsNameHelper.StorageOptionsSettingsName)
    .Get<StorageOptions>()!;

builder.Services.AddScoped<ISettingsService, StorageSettingsService>(_ =>
    new StorageSettingsService(storageOptions.Container, storageOptions.ConnectionString));

builder.Services.AddScoped<IAuditEventService, AuditEventTableService>(_ =>
    new AuditEventTableService(storageOptions.TableName, storageOptions.ConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/" + ConstantRouteHelper.HealthRoute, new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).AllowAnonymous();
    endpoints.MapControllers();
});
app.Run();