using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
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
        logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        //get environment variables
        var tenantId = Environment.GetEnvironmentVariable("TenantId");
        tenantId.ThrowIfNullOrEmpty();
        var clientId = Environment.GetEnvironmentVariable("ClientId");
        clientId.ThrowIfNullOrEmpty();
        var secret = Environment.GetEnvironmentVariable("Secret");
        clientId.ThrowIfNullOrEmpty();
        var dce = Environment.GetEnvironmentVariable("DataCollectionEndpointUrl");
        dce.ThrowIfNullOrEmpty();
        var dcr = Environment.GetEnvironmentVariable("DataCollectionRuleId");
        dcr.ThrowIfNullOrEmpty();
        var streamName = Environment.GetEnvironmentVariable("StreamName");
        streamName.ThrowIfNullOrEmpty();
        //app information
        var credential = new ClientSecretCredential(tenantId, clientId, secret);
        var logClient =
            new LogsIngestionClient(new Uri(dce!, UriKind.RelativeOrAbsolute), credential);

        // SIMULATING API call
        var currentTime = DateTimeOffset.UtcNow;
        var random = new Random().Next(1, 2200);
        var data = BinaryData.FromObjectAsJson(
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

        var response = await logClient.UploadAsync(dcr, streamName, RequestContent.Create(data));

        if (response.IsError)
            logger.LogError(response.ReasonPhrase);
        else
            logger.LogInformation("Data sent to log analytics at {DateCreated}", DateTime.Now);
    }
}