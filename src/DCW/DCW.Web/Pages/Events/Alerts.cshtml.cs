using DCW.Models;
using DCW.Shared;
using DCW.Web.Services;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class AlertsPageModel(ILogger<AlertsPageModel> logger, AuditEventHttpService auditEventHttpService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int? pageNumber)
    {
        logger.LogInformation("Loading index page for alerts {DateCreated}", DateTime.Now);
        var page = pageNumber ?? 1;
        var alarms = await auditEventHttpService.GetAlarmsAsync(page, Query);
        logger.LogInformation("Loaded alerts pages {AlarmCount} for alerts {DateCreated}", alarms.Count, DateTime.Now);
        AlarmModel = alarms;
        if (!Request.IsHtmx()) return Page();
        
        return Partial("_AlarmResult", AlarmModel);
    }

    [BindProperty(SupportsGet = true)] public string Query { get; set; } = "";
    [BindProperty] public PaginatedList<AuditEvent>? AlarmModel { get; set; }
}