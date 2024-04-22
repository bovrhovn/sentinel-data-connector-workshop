using DCW.Models;

namespace DCW.Interfaces;

public interface ISettingsService
{
    Task<bool> UpdateAsync(Settings settings);
    Task<Settings> GetAsync(string settingsId);
}