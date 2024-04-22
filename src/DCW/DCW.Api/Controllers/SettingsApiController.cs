using System.Net.Mime;
using DCW.Api.Authentication;
using DCW.Interfaces;
using DCW.Models;
using DCW.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DCW.Api.Controllers;

[Route(ConstantRouteHelper.SettingsBaseRoute)]
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
public class SettingsApiController(ISettingsService settingsService, ILogger<SettingsApiController> logger) : ControllerBase
{
    [HttpGet]
    [Route(ConstantRouteHelper.GetSettingsRoute + "/{settingsId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(Settings))]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetSettingsForUserAsync(string settingsId)
    {
        logger.LogInformation("Calling setting for user {SettingsId}", settingsId);
        var settings = await settingsService.GetAsync(settingsId);
        logger.LogInformation("settings for user {SettingsId} returned", settingsId);
        return Ok(settings);
    }

    [HttpPost]
    [Route(ConstantRouteHelper.SaveSettingsRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSettingsAsync([FromBody]Settings settings)
    {
        logger.LogInformation("Calling save settings for user {SettingsId}", settings.SettingsId);
        if (await settingsService.UpdateAsync(settings))
        {
            logger.LogInformation("Settings for user {SettingsId} has been saved", settings.SettingsId);
            return Ok();
        }

        logger.LogInformation("Settings for user {SettingsId} has not been saved", settings.SettingsId);
        return BadRequest("Settings has not been saved");
    }
}