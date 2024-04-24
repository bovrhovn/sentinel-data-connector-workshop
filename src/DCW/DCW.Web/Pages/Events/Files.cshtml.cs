using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class FilesPageModel(ILogger<FilesPageModel> logger, AuditEventHttpService auditEventHttpService) : PageModel
{
    public void OnGetAsync(int? pageNumber) => logger.LogInformation("Loading index page for files {DateCreated}", DateTime.Now);

    public async Task<IActionResult> OnGetFilterAsync(string query)
    {
        logger.LogInformation("Loading files for query {Query} at {DateLoaded}", query, DateTime.Now);
        var data = await auditEventHttpService.GetFilesAsync(query);
        logger.LogInformation("Loaded files results {FilesCount}", data.Count);
        return new JsonResult(data);
    }
}