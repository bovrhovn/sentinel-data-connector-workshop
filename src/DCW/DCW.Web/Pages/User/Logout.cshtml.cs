using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.User;

public class LogoutPageModel(ILogger<LogoutPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("Loading logout page at {DateLoaded}", DateTime.Now);

    public async Task<RedirectToPageResult> OnPostAsync()
    {
        logger.LogInformation("Logging current user out");
        await HttpContext.SignOutAsync();
        logger.LogInformation("User has been logged out at {DateLoggedOut}", DateTime.Now);
        
        return RedirectToPage("/Info/Index");
    }
}