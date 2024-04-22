using DCW.Models;
using DCW.Shared;
using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.User;

public class ProfilePageModel(ILogger<ProfilePageModel> logger, SettingHttpService settingHttpService) : PageModel
{
    public async Task OnGetAsync()
    {
        var profileName = User.Identity?.Name!;
        logger.LogInformation("Starting profile settings for user {Name}", profileName);
        var id = profileName.GetUniqueValue();
        logger.LogInformation("Getting data for user {Id} with {SettingsId}", profileName, id);
        UserSettings = await settingHttpService.GetSettingsForUserAsync(id);
        logger.LogInformation("Load profile settings at {DateLoaded}", DateTime.Now);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.Identity?.Name!;
        var settingsId = userId.GetUniqueValue();
        logger.LogInformation("Saving data to storage for user {Id} with settings id {SettingsId}", userId, settingsId);
        UserSettings.SettingsId = settingsId;
        await settingHttpService.UpdateAsync(UserSettings);
        logger.LogInformation("Data saved to storage for user {Id} with {SettingsId} identificator", userId,
            settingsId);
        return RedirectToPage("/User/Profile");
    }

    [BindProperty] public Settings UserSettings { get; set; } = new();
}