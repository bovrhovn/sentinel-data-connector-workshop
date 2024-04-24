using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class AlertsPageModel(ILogger<AlertsPageModel> logger, AuditEventHttpService auditEventHttpService) : PageModel
{
    public void OnGet(int? pageNumber) => logger.LogInformation("Loading index page for alerts {DateCreated}", DateTime.Now);

    public async Task<IActionResult> OnGetFilterAsync(string query)
    {
        logger.LogInformation("Loading files for query {Query} at {DateLoaded}", query, DateTime.Now);
        var data = await auditEventHttpService.GetAlarmsAsync(query);
        logger.LogInformation("Loaded files results {FilesCount}", data.Count);
        return new JsonResult(data);
    }
}