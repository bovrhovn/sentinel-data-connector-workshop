namespace DCW.Models;

public class AuditEvent
{
    public string AuditEventId { get; set; }
    public AuditEventTypes EventType { get; set; } = AuditEventTypes.ALERT;
    public string Message { get; set; }
    public string SourceIp { get; set; }
    public string DestinationIp { get; set; }
    public DateTime TimeStamp { get; set; }
}

public enum AuditEventTypes
{
    ALERT = 0,
    FILE
}