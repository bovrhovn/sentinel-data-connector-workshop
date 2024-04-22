using DCW.Web.Models;
using DCW.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Events;

public class AddPageModel(SecurityEventsHttpService securityEventsHttpService, ILogger<AddPageModel> logger) : PageModel
{
    public void OnGet() => logger.LogInformation("OnGet called at {DateCreated}", DateTime.Now);

    public async Task<IActionResult> OnPostAsync()
    {
        logger.LogInformation("OnPost called at {DateCreated}", DateTime.Now);
        if (EventModel == null) return Page();
        await securityEventsHttpService.SendEventData(EventModel);
        logger.LogInformation("Data has been saved");
        return RedirectToPage("/Events/Index");
    }

    [BindProperty] public EventDataModel EventModel { get; set; } = new("");
}