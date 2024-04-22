using DCW.Models;
using DCW.Shared;
using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.User;

public class NotificationsPageModel(ILogger<NotificationsPageModel> logger, SettingHttpService settingHttpService)
    : PageModel
{
    public async Task OnGet()
    {
        logger.LogInformation("Loaded events page at {DateLoaded}", DateTime.UtcNow);
        var profileName = User.Identity?.Name!;
        logger.LogInformation("Starting profile settings for user {Name}", profileName);
        var id = profileName.GetUniqueValue();
        var settings = await settingHttpService.GetSettingsForUserAsync(id);
        logger.LogInformation("Getting data for user {Id} with {SettingsId}", profileName, id);
        UserSettings = settings;
    }

    [BindProperty] public Settings? UserSettings { get; set; }
}