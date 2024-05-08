using System.Net.Http.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using DCW.Models;
using DCW.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DCW.AuditEventFunc;

public class GetAuditEvents(ILoggerFactory loggerFactory)
{
    private readonly ILogger logger = loggerFactory.CreateLogger<GetAuditEvents>();

    [Function("GetAuditEvents")]
    public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation($"Get audit events executed: {DateTime.UtcNow}");
        //get environment variables

        #region Environment variables

        var tenantId = Environment.GetEnvironmentVariable("TenantId");
        var clientId = Environment.GetEnvironmentVariable("ClientId");
        var secret = Environment.GetEnvironmentVariable("Secret");
        
        var dce = Environment.GetEnvironmentVariable("DataCollectionEndpointUrl");
        dce.ThrowIfNullOrEmpty();
        var dcr = Environment.GetEnvironmentVariable("DataCollectionRuleId");
        dcr.ThrowIfNullOrEmpty();
        var streamName = Environment.GetEnvironmentVariable("StreamName");
        streamName.ThrowIfNullOrEmpty();
        var apiUrl = Environment.GetEnvironmentVariable("ApiBaseUrl");

        #endregion

        TokenCredential credential = new DefaultAzureCredential();
        if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(secret))
        {
            logger.LogError("TenantId, ClientId and Secret are not set. Please set them in the environment variables to sign with app center");
            credential = new ClientSecretCredential(tenantId, clientId, secret);
        }
        //app information
        var logClient =
            new LogsIngestionClient(new Uri(dce!, UriKind.RelativeOrAbsolute), credential);

        // call my API
        var currentTime = DateTimeOffset.UtcNow;
        BinaryData? data = null;
        if (string.IsNullOrEmpty(apiUrl))
        {
            // SIMULATING API call
            var random = new Random().Next(1, 2200);
            data = BinaryData.FromObjectAsJson(
                new[]
                {
                    new
                    {
                        TimeGenerated = currentTime,
                        Message = $"this is a message from system {random}",
                        AuditEventId = Guid.NewGuid().ToString(),
                        SourceIp = "127.0.0.1",
                        DestinationIp = "192.168.1.1"
                    }
                });
        }
        else
        {
            // call the API from production
            var client = new HttpClient { BaseAddress = new Uri(apiUrl, UriKind.RelativeOrAbsolute) };
            var currentResponse =
                await client.GetAsync(
                    $"{ConstantRouteHelper.AuditEventsBaseRoute}/{ConstantRouteHelper.GetAlarmsRoute}");
            if (currentResponse.IsSuccessStatusCode)
            {
                var alarms = await currentResponse.Content.ReadFromJsonAsync<List<AuditEvent>>();
                logger.LogInformation("Get alarms from service {AlarmCount}", alarms?.Count);

                if (alarms == null)
                    logger.LogError("Error getting alarms from service");
                else
                {
                    foreach (var currentAlarm in alarms)
                    {
                        data = BinaryData.FromObjectAsJson(
                            new[]
                            {
                                new
                                {
                                    TimeGenerated = currentAlarm.TimeStamp,
                                    Message = currentAlarm.Message,
                                    AuditEventId = currentAlarm.AuditEventId,
                                    SourceIp = currentAlarm.SourceIp,
                                    DestinationIp = currentAlarm.DestinationIp
                                }
                            });
                    }
                }
            }
            else
                logger.LogError(
                    "There is an error with calling the service. No data returned and response code was not success");
        }

        if (data == null)
        {
            logger.LogError("Data is null. No data to send to log analytics - at {DateCreated}", DateTime.Now);
            return;
        }

        var response = await logClient.UploadAsync(dcr, streamName, RequestContent.Create(data));

        if (response.IsError)
            logger.LogError(response.ReasonPhrase);
        else
            logger.LogInformation("Data sent to log analytics at {DateCreated}", DateTime.Now);
    }
}