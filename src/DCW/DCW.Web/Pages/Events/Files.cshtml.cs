using DCW.Models;
using DCW.Shared;
using DCW.Web.Services;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class FilesPageModel(ILogger<FilesPageModel> logger, AuditEventHttpService auditEventHttpService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int? pageNumber)
    {
        logger.LogInformation("Loading index page for files {DateCreated}", DateTime.Now);
        var page = pageNumber ?? 1;
        var filesModel = await auditEventHttpService.GetFilesAsync(page, Query);
        logger.LogInformation("Loaded files pages {FilesCount} for files {DateCreated}", filesModel.Count, DateTime.Now);
        FilesModel = filesModel;
        if (!Request.IsHtmx()) return Page();
        
        return Partial("_FilesResult", FilesModel);
    }

    [BindProperty(SupportsGet = true)] public string Query { get; set; } = "";
    [BindProperty] public PaginatedList<AuditEventFile>? FilesModel { get; set; }
}