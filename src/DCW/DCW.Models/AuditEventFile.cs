namespace DCW.Models;

public class AuditEventFile : AuditEvent
{
    public string FileType { get; set; }
    public int FileSizeBytes { get; set; }
}