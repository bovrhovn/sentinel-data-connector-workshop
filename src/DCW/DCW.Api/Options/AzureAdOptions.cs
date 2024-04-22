namespace DCW.Api.Options;

public class AzureAdOptions
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = "onmicrosoft.com";
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string SubscriptionId { get; set; } = "";
    public string Secret { get; set; } = "";
}