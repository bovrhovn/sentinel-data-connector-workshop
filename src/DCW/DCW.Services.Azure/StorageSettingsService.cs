using System.Diagnostics;
using System.Text;
using Azure.Storage.Blobs;
using DCW.Interfaces;
using DCW.Models;
using Newtonsoft.Json;

namespace DCW.Services.Azure;

public class StorageSettingsService(string profileContainerName, string connectionString) : ISettingsService
{
    public async Task<bool> UpdateAsync(Settings settings)
    {
        var currentSettings = await GetAsync(settings.SettingsId);
        currentSettings.AllowNotifications = settings.AllowNotifications;

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(profileContainerName);
        try
        {
            var blobClient = containerClient.GetBlobClient(settings.SettingsId);
            var data = JsonConvert.SerializeObject(settings);
            var bytes = Encoding.UTF8.GetBytes(data);
            using var ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            await blobClient.UploadAsync(ms, true);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }

        return true;
    }

    public async Task<Settings> GetAsync(string settingsId)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(profileContainerName);
        var settings = new Settings { SettingsId = settingsId };
        if (!await containerClient.ExistsAsync())
            await blobServiceClient.CreateBlobContainerAsync(profileContainerName);
        var blobClient = containerClient.GetBlobClient(settingsId);
        if (!await blobClient.ExistsAsync())
            return settings;
        var downloadedContent = await blobClient.DownloadContentAsync();
        if (!downloadedContent.HasValue) return settings;
        var downloadedProfile = Encoding.UTF8.GetString(downloadedContent.Value.Content);
        if (string.IsNullOrEmpty(downloadedProfile)) return settings;
        settings = JsonConvert.DeserializeObject<Settings>(downloadedProfile);
        return settings ?? new Settings
        {
            SettingsId = settingsId, DataFrequencyRetrievalInSeconds = 60, DefaultRecordSize = 100,
            AllowNotifications = false
        };
    }
}