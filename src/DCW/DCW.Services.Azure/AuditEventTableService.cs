using System.Diagnostics;
using Azure.Data.Tables;
using DCW.Interfaces;
using DCW.Models;

namespace DCW.Services.Azure;

public class AuditEventTableService(string tableName, string connectionString) : IAuditEventService
{
    private readonly TableClient serviceClient = new(connectionString, tableName);

    public async Task<List<AuditEvent>> GetAlarmsAsync()
    {
        var alert = (int)AuditEventTypes.ALERT;
        var results =
            serviceClient.QueryAsync<AuditEventTableEntity>(filter => filter.EventType == alert);
        var auditEvents = new List<AuditEvent>();
        await foreach (var result in results.AsPages())
        {
            var currentValueFromDatabase = result.Values;
            foreach (var auditEventTableEntity in currentValueFromDatabase)
            {
                var ae = new AuditEvent
                {
                    AuditEventId = auditEventTableEntity.AuditEventId,
                    Message = auditEventTableEntity.Message,
                    SourceIp = auditEventTableEntity.SourceIp,
                    DestinationIp = auditEventTableEntity.DestinationIp,
                    TimeStamp = auditEventTableEntity.Timestamp!.Value.DateTime,
                    EventType = AuditEventTypes.ALERT
                };
                auditEvents.Add(ae);
            }
        }

        return auditEvents;
    }

    public async Task<List<AuditEventFile>> GetFilesAsync()
    {
        var results =
            serviceClient.QueryAsync<AuditEventTableEntity>(filter => filter.EventType == (int)AuditEventTypes.FILE);
        var auditEvents = new List<AuditEventFile>();
        await foreach (var result in results.AsPages())
        {
            var currentValueFromDatabase = result.Values.FirstOrDefault();
            if (currentValueFromDatabase == null) continue;
            var ae = new AuditEventFile
            {
                AuditEventId = currentValueFromDatabase.AuditEventId,
                Message = currentValueFromDatabase.Message,
                SourceIp = currentValueFromDatabase.SourceIp,
                DestinationIp = currentValueFromDatabase.DestinationIp,
                TimeStamp = currentValueFromDatabase.Timestamp!.Value.DateTime,
                EventType = AuditEventTypes.FILE,
                FileType = currentValueFromDatabase.FileType,
                FileSizeBytes = currentValueFromDatabase.FileSizeBytes
            };
            auditEvents.Add(ae);
        }

        return auditEvents;
    }

    public async Task<bool> InsertAsync(AuditEvent auditEvent)
    {
        var aeTableEntity = new AuditEventTableEntity
        {
            PartitionKey = tableName,
            AuditEventId = auditEvent.AuditEventId,
            Message = auditEvent.Message,
            Timestamp = auditEvent.TimeStamp,
            SourceIp = auditEvent.SourceIp,
            DestinationIp = auditEvent.DestinationIp,
            EventType = (int)auditEvent.EventType,
            FileSizeBytes = 0,
            FileType = string.Empty
        };

        try
        {
            await serviceClient.CreateIfNotExistsAsync();
            await serviceClient.AddEntityAsync(aeTableEntity);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> InsertBulkAsync(List<AuditEvent> auditEvents)
    {
        var actions = new List<TableTransactionAction>();
        foreach (var auditEvent in auditEvents)
        {
            var ae = new AuditEventTableEntity
            {
                PartitionKey = tableName,
                AuditEventId = auditEvent.AuditEventId,
                Message = auditEvent.Message,
                Timestamp = auditEvent.TimeStamp,
                SourceIp = auditEvent.SourceIp,
                DestinationIp = auditEvent.DestinationIp,
                EventType = (int)auditEvent.EventType
            };
            
            if (auditEvent.EventType == AuditEventTypes.FILE)
            {
                var aeFile = (AuditEventFile)auditEvent;
                ae.FileSizeBytes = aeFile.FileSizeBytes;
                ae.FileType = aeFile.FileType;
            }

            actions.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, ae));
        }

        try
        {
            await serviceClient.CreateIfNotExistsAsync();
            await serviceClient.SubmitTransactionAsync(actions);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAsync(string eventId)
    {
        try
        {
            var ae = await GetAsync(eventId);
            await serviceClient.DeleteEntityAsync(ae.EventType.ToString(), eventId);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateAsync(AuditEvent auditEvent)
    {
        var aeTableEntity = new AuditEventTableEntity
        {
            PartitionKey = tableName,
            AuditEventId = auditEvent.AuditEventId,
            Message = auditEvent.Message,
            Timestamp = auditEvent.TimeStamp,
            SourceIp = auditEvent.SourceIp,
            DestinationIp = auditEvent.DestinationIp,
            EventType = (int)auditEvent.EventType,
            FileSizeBytes = 0,
            FileType = string.Empty
        };
        try
        {
            await serviceClient.UpsertEntityAsync(aeTableEntity, TableUpdateMode.Replace);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return false;
        }

        return true;
    }

    public async Task<AuditEvent> GetAsync(string eventId)
    {
        try
        {
            var entity = await serviceClient.GetEntityAsync<AuditEventTableEntity>(tableName, eventId);
            var currentValueFromDatabase = entity.Value;
            var ae = new AuditEvent
            {
                AuditEventId = currentValueFromDatabase.AuditEventId,
                Message = currentValueFromDatabase.Message,
                SourceIp = currentValueFromDatabase.SourceIp,
                DestinationIp = currentValueFromDatabase.DestinationIp,
                TimeStamp = currentValueFromDatabase.Timestamp!.Value.DateTime
            };
            var eventTypes = Enum.Parse<AuditEventTypes>(currentValueFromDatabase.EventType.ToString(), true);
            if (eventTypes == AuditEventTypes.ALERT)
            {
                ae.EventType = AuditEventTypes.ALERT;
                return ae;
            }

            ae.EventType = AuditEventTypes.FILE;
            var aeFile = new AuditEventFile
            {
                AuditEventId = currentValueFromDatabase.AuditEventId,
                Message = currentValueFromDatabase.Message,
                SourceIp = currentValueFromDatabase.SourceIp,
                DestinationIp = currentValueFromDatabase.DestinationIp,
                TimeStamp = currentValueFromDatabase.Timestamp!.Value.DateTime,
                FileSizeBytes = currentValueFromDatabase.FileSizeBytes,
                FileType = currentValueFromDatabase.FileType
            };
            return aeFile;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return null!;
        }
    }
}