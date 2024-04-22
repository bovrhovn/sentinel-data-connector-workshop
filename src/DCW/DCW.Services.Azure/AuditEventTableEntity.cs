using Azure;
using Azure.Data.Tables;

namespace DCW.Services.Azure;

public class AuditEventTableEntity : ITableEntity
{
    public string AuditEventId
    {
        get => RowKey;
        set => RowKey = value;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string FileType { get; set; }
    public int FileSizeBytes { get; set; }
    public int EventType { get; set; }
    public string Message { get; set; }
    public string SourceIp { get; set; }
    public string DestinationIp { get; set; }
}