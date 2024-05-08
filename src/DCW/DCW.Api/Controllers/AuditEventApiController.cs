using System.Net.Mime;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Bogus;
using DCW.Api.Authentication;
using DCW.Api.Models;
using DCW.Api.Options;
using DCW.Interfaces;
using DCW.Models;
using DCW.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DCW.Api.Controllers;

[Route(ConstantRouteHelper.AuditEventsBaseRoute)]
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
public class AuditEventApiController(
    ILogger<AuditEventApiController> logger,
    IOptions<AzureMonOptions> azureMonOptionsValue,
    IOptions<AzureAdOptions> azureOptionsValue,
    IAuditEventService auditEventService) : ControllerBase
{
    [HttpGet]
    [Route(ConstantRouteHelper.HealthRoute)]
    [AllowAnonymous]
    public IActionResult CheckHealth()
    {
        logger.LogInformation("Checking health at {DateCreated}", DateTime.Now);
        return Ok($"I am healthy at {DateTime.Now}");
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetEventsRoute + "/{resultCount}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(List<AuditEvent>))]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetSecurityEventsAsync(int resultCount = 20)
    {
        logger.LogInformation("Calling Azure Log Analytics for security events at {DateCreated}", DateTime.Now);
        var azureAdOptions = azureOptionsValue.Value;
        var credential = new ClientSecretCredential(azureAdOptions.TenantId, azureAdOptions.ClientId,
            azureAdOptions.Secret);

        LogsQueryClient logsQueryClient = new(credential);
        LogsBatchQuery batch = new();

        var valueWorkspaceId = azureMonOptionsValue.Value.WorkspaceId;
        logger.LogInformation("Reading data for the workspace {WorkSpaceId} at {DateCreated}", valueWorkspaceId,
            DateTime.Now);
        var logsQueryOptions = new LogsQueryOptions { IncludeStatistics = true };
        var countQueryId = batch.AddWorkspaceQuery(valueWorkspaceId,
            azureMonOptionsValue.Value.TableName + " | count",
            new QueryTimeRange(TimeSpan.FromDays(7)), logsQueryOptions);

        logger.LogInformation("Setting up count query for audit events at {DateCreated}", DateTime.Now);

        var data = batch.AddWorkspaceQuery(valueWorkspaceId,
            azureMonOptionsValue.Value.TableName + $" | take {resultCount}",
            new QueryTimeRange(TimeSpan.FromDays(7)), logsQueryOptions);

        logger.LogInformation("Setting up data query for events at {DateCreated}", DateTime.Now);
        Response<LogsBatchQueryResultCollection> queryResponse = await logsQueryClient.QueryBatchAsync(batch);
        logger.LogInformation("Query called at {DateCreated}", DateTime.Now);
        var list = new List<AuditEvent>();
        try
        {
            var count = queryResponse.Value.GetResult<long>(countQueryId).Single();
            if (count == 0) return Ok(list);
            logger.LogInformation("Received {Count} events at {DateCreated}", count, DateTime.Now);
            var topEntries = queryResponse.Value.GetResult<AuditEventViewModel>(data);
            logger.LogInformation("Returning {Count} events at {DateCreated}", topEntries.Count, DateTime.Now);
            list = topEntries.Select(currentEntry => new AuditEvent
            {
                AuditEventId = currentEntry.AuditEventId,
                Message = currentEntry.Message,
                SourceIp = currentEntry.SourceIp,
                DestinationIp = currentEntry.DestinationIp,
                TimeStamp = currentEntry.TimeStamp
            }).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
        return Ok(list);
    }

    [HttpPost]
    [Route(ConstantRouteHelper.AddEventRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddEventAsync([FromBody] MyEvent myEvent)
    {
        // https://github.com/Azure-Samples/azure-samples-net-management
        logger.LogInformation("Sending {Message} to the log analytics at {DateCreated}", myEvent.Message,
            DateTime.Now);
        var azureAdOptions = azureOptionsValue.Value;
        var credential = new ClientSecretCredential(azureAdOptions.TenantId, azureAdOptions.ClientId,
            azureAdOptions.Secret);
        var azureMonOptions = azureMonOptionsValue.Value;
        var logClient =
            new LogsIngestionClient(new Uri(azureMonOptions.DataCollectionEndpointUrl, UriKind.RelativeOrAbsolute),
                credential);

        var streamName = azureMonOptions.StreamName;

        var currentTime = DateTimeOffset.UtcNow;
        var data = BinaryData.FromObjectAsJson(
            new[]
            {
                new
                {
                    TimeGenerated = currentTime,
                    Message = myEvent.Message,
                    AuditEventId = Guid.NewGuid().ToString(),
                    SourceIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DestinationIp = "192.168.1.1"
                }
            });

        var response = await logClient.UploadAsync(azureMonOptions.DataCollectionRuleId, streamName,
            RequestContent.Create(data));

        if (!response.IsError) return Ok();

        logger.LogError("Error sending {Message} to the log analytics at {DateCreated}", myEvent.Message,
            DateTime.Now);
        return BadRequest();
    }

    [Route(ConstantRouteHelper.GenerateAuditEventsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    [Produces(typeof(AuditEvent))]
    public async Task<IActionResult> GenerateAsync([FromBody] GenerateOptions generateOptions)
    {
        logger.LogInformation("Generate {RecordNumber} events at {DateCreated}", generateOptions.RecordNumber,
            DateTime.Now);
        //generate objects
        var list = new List<AuditEvent>();
        for (var currentRecord = 0; currentRecord < generateOptions.RecordNumber; currentRecord++)
        {
            //generate records  
            var aeAuditEventId = Guid.NewGuid().ToString();
            if (currentRecord % 2 == 0)
            {
                var ae = new Faker<AuditEvent>()
                    .RuleFor(auditEvent => auditEvent.TimeStamp,
                        f => f.Date.Between(DateTime.Now.AddMonths(new Random().Next(6, 12)),
                            DateTime.Now.AddMonths(new Random().Next(1, 5))))
                    .RuleFor(auditEvent => auditEvent.Message, f => f.Hacker.Phrase())
                    .RuleFor(auditEvent => auditEvent.SourceIp, f => f.Internet.IpAddress().ToString())
                    .RuleFor(auditEvent => auditEvent.DestinationIp, f => f.Internet.IpAddress().ToString())
                    .Generate();
                ae.AuditEventId = aeAuditEventId;
                ae.EventType = AuditEventTypes.ALERT;
                list.Add(ae);
            }
            else
            {
                var ae = new Faker<AuditEventFile>()
                    .RuleFor(auditEvent => auditEvent.TimeStamp, f =>
                        f.Date.Between(DateTime.Now.AddMonths(new Random().Next(6, 12)),
                            DateTime.Now.AddMonths(new Random().Next(1, 5))))
                    .RuleFor(auditEvent => auditEvent.Message, f => f.Hacker.Phrase())
                    .RuleFor(auditEvent => auditEvent.SourceIp, f => f.Internet.IpAddress().ToString())
                    .RuleFor(auditEvent => auditEvent.FileType, f => f.System.FileExt())
                    .RuleFor(auditEvent => auditEvent.FileSizeBytes, f => f.Random.Int(100, 1000000))
                    .RuleFor(auditEvent => auditEvent.DestinationIp, f => f.Internet.IpAddress().ToString())
                    .Generate();
                ae.AuditEventId = aeAuditEventId;
                ae.EventType = AuditEventTypes.FILE;

                list.Add(ae);
            }
        }

        if (generateOptions.IsPersistent)
        {
            logger.LogInformation("Persisting {RecordNumber} events at {DateCreated}", generateOptions.RecordNumber,
                DateTime.Now);
            if (await auditEventService.InsertBulkAsync(list))
                logger.LogInformation("Persisted {RecordNumber} events at {DateCreated}",
                    generateOptions.RecordNumber,
                    DateTime.Now);
        }

        logger.LogInformation("Generated {RecordNumber} events at {DateCreated}", generateOptions.RecordNumber,
            DateTime.Now);
        return Ok(list);
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetAlarmsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(AuditEvent))]
    //[ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetAlarmsAsync()
    {
        logger.LogInformation("Calling Azure Table Storage for alarms at {DateCreated}", DateTime.Now);
        var auditEvents = await auditEventService.GetAlarmsAsync();
        logger.LogInformation("Received {Count} alarms at {DateCreated}", auditEvents.Count, DateTime.Now);
        return Ok(auditEvents);
    }

    [HttpGet]
    [Route(ConstantRouteHelper.GetFilesRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(AuditEventFile))]
    // [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetFilesAsync()
    {
        logger.LogInformation("Calling Azure Table Storage for files at {DateCreated}", DateTime.Now);
        var auditEvents = await auditEventService.GetFilesAsync();
        logger.LogInformation("Received {Count} files at {DateCreated}", auditEvents.Count, DateTime.Now);
        return Ok(auditEvents);
    }
}