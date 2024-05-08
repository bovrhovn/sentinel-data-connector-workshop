namespace DCW.Api.Models;

public class AuditEventViewModel
{
    public string AuditEventId { get; set; }
    public string Message { get; set; }
    public string SourceIp { get; set; }
    public string DestinationIp { get; set; }
    public DateTime TimeStamp { get; set; }
}