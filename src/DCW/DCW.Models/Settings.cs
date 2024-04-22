namespace DCW.Models;

public class Settings
{
    public string SettingsId { get; set; }
    public bool AllowNotifications { get; set; }
    public int DataFrequencyRetrievalInSeconds { get; set; } = 60;
    public int DefaultRecordSize { get; set; } = 300;
}