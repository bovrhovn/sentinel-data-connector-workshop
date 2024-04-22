using DCW.Models;
using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class IndexPageModel(SecurityEventsHttpService securityEventsHttpService, ILogger<IndexPageModel> logger)
    : PageModel
{
    public async Task OnGetAsync()
    {
        logger.LogInformation("Loaded events page at {DateLoaded}", DateTime.UtcNow);
        if (string.IsNullOrEmpty(Query))
        {
            logger.LogInformation("Query is empty at {DateLoaded}, returning 20 results", DateTime.UtcNow);
            AuditEvents = await securityEventsHttpService.GetEventDataAsync(20);
            logger.LogInformation("Loaded {Count} events at {DateLoaded}", AuditEvents.Count, DateTime.UtcNow);
        }
    }

    [BindProperty] public List<AuditEvent> AuditEvents { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string Query { get; set; } = "";
}