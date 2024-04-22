using System.ComponentModel.DataAnnotations;

namespace DCW.Web.Options;

public class AppOptions
{
    [Required(ErrorMessage = "ApiUrl is required")]
    public string ApiUrl { get; set; } = "";
}