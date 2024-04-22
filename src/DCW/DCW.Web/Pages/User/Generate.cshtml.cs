using DCW.Models;
using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.User;

public class GeneratePageModel(ILogger<GeneratePageModel> logger, AuditEventHttpService auditEventHttpService) : PageModel
{
    public void OnGet() => logger.LogInformation("Generate page called at {DateCreated}", DateTime.Now);

    public async Task<IActionResult> OnPostAsync()
    {
        logger.LogInformation("Generate page POST method called at {DateCreated}", DateTime.Now);
        await auditEventHttpService.GenerateAsync(GenerateOptions);
        logger.LogInformation("Generate page POST method finished at {DateCreated}", DateTime.Now);
        return RedirectToPage("/Info/Index");
    }

    [BindProperty] public GenerateOptions GenerateOptions { get; set; } = new();
}