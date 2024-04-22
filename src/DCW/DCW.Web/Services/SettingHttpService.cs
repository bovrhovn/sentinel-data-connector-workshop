using DCW.Models;
using DCW.Shared;
using DCW.Web.Options;
using Microsoft.Extensions.Options;

namespace DCW.Web.Services;

public class SettingHttpService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SettingHttpService> logger;

    public SettingHttpService(HttpClient httpClient,
        IOptions<AppOptions> appOptions,
        IOptions<AuthOptions> authOptionsValue,
        ILogger<SettingHttpService> logger)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(appOptions.Value.ApiUrl);
        this.httpClient.DefaultRequestHeaders.Add(AuthOptions.ApiKeyHeaderName, authOptionsValue.Value.ApiKey);
        this.logger = logger;
    }

    public async Task<Settings> GetSettingsForUserAsync(string settingsId)
    {
        logger.LogInformation("Calling setting for user {SettingsId}", settingsId);
        var response =
            await httpClient.GetAsync(
                $"{ConstantRouteHelper.SettingsBaseRoute}/{ConstantRouteHelper.GetSettingsRoute}/{settingsId}");
        if (!response.IsSuccessStatusCode) throw new Exception($"Error getting settings for user {settingsId}");
        var settings = await response.Content.ReadFromJsonAsync<Settings>();
        return settings!;
    }

    public async Task<bool> UpdateAsync(Settings userSettings)
    {
        logger.LogInformation("Saving setting for user {SettingsId}", userSettings.SettingsId);
        var response = await httpClient.PostAsJsonAsync(
            $"{ConstantRouteHelper.SettingsBaseRoute}/{ConstantRouteHelper.SaveSettingsRoute}", userSettings);
        logger.LogInformation("Settings saved for user {SettingsId} has been updated", userSettings.SettingsId);
        return response.IsSuccessStatusCode;
    }
}