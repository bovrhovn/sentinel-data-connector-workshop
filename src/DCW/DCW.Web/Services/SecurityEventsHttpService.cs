using DCW.Models;
using DCW.Shared;
using DCW.Web.Models;
using DCW.Web.Options;
using Microsoft.Extensions.Options;

namespace DCW.Web.Services;

public class SecurityEventsHttpService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SettingHttpService> logger;

    public SecurityEventsHttpService(HttpClient httpClient,
        IOptions<AppOptions> appOptions,
        IOptions<AuthOptions> authOptionsValue,
        ILogger<SettingHttpService> logger)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(appOptions.Value.ApiUrl);
        this.httpClient.DefaultRequestHeaders.Add(AuthOptions.ApiKeyHeaderName, authOptionsValue.Value.ApiKey);
        this.logger = logger;
    }

    public async Task<List<AuditEvent>> GetEventDataAsync(int fromDays)
    {
        logger.LogInformation("Calling events and getting data");
        var response =
            await httpClient.GetAsync(
                $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.GetEventsRoute}/{fromDays}");
        if (!response.IsSuccessStatusCode) throw new Exception($"Error getting audit events for {fromDays}");
        logger.LogInformation("Call has been made to events and data has been received");
        var eventData = await response.Content.ReadFromJsonAsync<List<AuditEvent>>();
        return eventData!;
    }

    public async Task<bool> SendEventData(EventDataModel data)
    {
        logger.LogInformation("Calling events for adding data");
        var response =
            await httpClient.PostAsJsonAsync(
                $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.AddEventRoute}", new MyEvent
                {
                    Message = data.Message
                });
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Error adding eventdata");
            return false;
        }

        logger.LogInformation("Call has been made to events and data has been saved");
        return true;
    }
}