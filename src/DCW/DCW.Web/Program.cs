using System.Net;
using DCW.Shared;
using DCW.Web.Options;
using DCW.Web.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Polly;
using Polly.Contrib.WaitAndRetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AuthOptionsSectionName))
    .ValidateDataAnnotations();
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(SettingsNameHelper.AppOptionsSectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<SettingHttpService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

builder.Services.AddHttpClient<AuditEventHttpService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

builder.Services.AddHttpClient<SecurityEventsHttpService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(SettingsNameHelper.AzureAdSettingsName));

builder.Services.AddControllers();
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
        options.Conventions.AddPageRoute("/Info/Index", ""))
    .AddMvcOptions(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    }).AddMicrosoftIdentityUI();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        if (exception != null)
        {
            var message = $"{exception.Error.Message}";
            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
    });
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
app.Run();