﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DCW.Web.Pages.Info;

public class PrivacyModel(ILogger<PrivacyModel> logger) : PageModel
{
    private readonly ILogger<PrivacyModel> _logger = logger;

    public void OnGet()
    {
    }
}