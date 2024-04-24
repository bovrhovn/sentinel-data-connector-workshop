using DCW.Models;
using DCW.Shared;
using DCW.Web.Options;
using Microsoft.Extensions.Options;

namespace DCW.Web.Services;

public class AuditEventHttpService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SettingHttpService> logger;

    public AuditEventHttpService(HttpClient httpClient,
        IOptions<AppOptions> appOptions,
        IOptions<AuthOptions> authOptionsValue,
        ILogger<SettingHttpService> logger)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(appOptions.Value.ApiUrl);
        this.httpClient.DefaultRequestHeaders.Add(AuthOptions.ApiKeyHeaderName, authOptionsValue.Value.ApiKey);
        this.logger = logger;
    }

    public async Task<List<AuditEvent>> GetAlarmsAsync(string query = "")
    {
        var response =
            await httpClient.GetAsync(
                $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.GetAlarmsRoute}");
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "There is an error with calling the service. No data returned and response code was not success");
            throw new Exception("Error getting alarms from service");
        }

        var alarms = await response.Content.ReadFromJsonAsync<List<AuditEvent>>();
        logger.LogInformation("Get alarms from service {AlarmCount}", alarms?.Count);

        if (alarms == null)
        {
            logger.LogError("Error getting alarms from service");
            throw new Exception("Error getting alarms from service");
        }

        if (string.IsNullOrEmpty(query)) return alarms;

        logger.LogInformation("Filtering alarms with query {Query}", query);
        alarms = alarms.Where(currentData => currentData.Message.Contains(query))
            .ToList();
        logger.LogInformation("After filtering alarms with query {Query} --> {AlarmCount} results", query,
            alarms.Count);

        return alarms;
    }

    public async Task<List<AuditEventFile>> GetFilesAsync(string query = "")
    {
        var response =
            await httpClient.GetAsync(
                $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.GetFilesRoute}");
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "There is an error with calling the service. No data returned and response code was not success");
            throw new Exception("Error getting alarms from service");
        }

        var filesResult = await response.Content.ReadFromJsonAsync<List<AuditEventFile>>();
        logger.LogInformation("Get files from service {AlarmCount}", filesResult?.Count);

        if (filesResult == null)
        {
            logger.LogError("Error getting files from service");
            throw new Exception("Error getting alarms from service");
        }

        if (string.IsNullOrEmpty(query))
            return filesResult;

        logger.LogInformation("Filtering files with query {Query}", query);
        filesResult = filesResult.Where(currentData => currentData.Message.Contains(query))
            .ToList();
        logger.LogInformation("After filtering files with query {Query} --> {AlarmCount} results", query,
            filesResult.Count);

        return filesResult;
    }

    public async Task<bool> GenerateAsync(GenerateOptions generateOptions)
    {
        logger.LogInformation("Generate records http called started at {DateCreated}", DateTime.Now);
        var result = await httpClient.PostAsJsonAsync(
            $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.GenerateAuditEventsRoute}",
            generateOptions);
        logger.LogInformation("Generate records http called finished at {DateCreated}", DateTime.Now);
        return result.IsSuccessStatusCode;
    }
}