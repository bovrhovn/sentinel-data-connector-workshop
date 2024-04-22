using System.ComponentModel.DataAnnotations;

namespace DCW.Api.Options;

public class AzureMonOptions
{
    [Required(ErrorMessage = "Workspace id is required")]
    public string WorkspaceId { get; set; }
    [Required(ErrorMessage = "Table to query is required")]
    public string TableName { get; set; }
    [Required(ErrorMessage = "Stream name is required")]
    public string StreamName { get; set; }
    [Required(ErrorMessage = "Data collection endpoint url is required to save the data")]
    public string DataCollectionEndpointUrl { get; set; }
    [Required(ErrorMessage = "Data collection rule id is required to save the data")]
    public string DataCollectionRuleId { get; set; }
}