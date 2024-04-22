using System.ComponentModel.DataAnnotations;

namespace DCW.Shared;

public class AuthOptions
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    [Required(ErrorMessage = "Api key must be defined")] 
    public string ApiKey { get; set; }
}