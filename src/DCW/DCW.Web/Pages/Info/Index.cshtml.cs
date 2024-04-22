using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Info;

[AllowAnonymous]
public class IndexPageModel(ILogger<IndexPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("Loaded info page at {DateLoaded}", DateTime.Now);
}