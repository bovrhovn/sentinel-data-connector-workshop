using DCW.Models;

namespace DCW.Interfaces;

public interface IAuditEventService
{
    Task<List<AuditEvent>> GetAlarmsAsync();
    Task<List<AuditEventFile>> GetFilesAsync();
    Task<bool> InsertAsync(AuditEvent auditEvent);
    Task<bool> InsertBulkAsync(List<AuditEvent> auditEvents);
    Task<bool> DeleteAsync(string eventId);
    Task<bool> UpdateAsync(AuditEvent auditEvent);
    Task<AuditEvent> GetAsync(string eventId);
}