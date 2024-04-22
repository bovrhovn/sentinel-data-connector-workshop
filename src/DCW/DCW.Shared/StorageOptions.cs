using System.ComponentModel.DataAnnotations;

namespace DCW.Shared;

public class StorageOptions
{
    [Required(ErrorMessage = "Name of container is required")]
    public string Container { get; set; }

    [Required(ErrorMessage = "Connection string is required")]
    public string ConnectionString { get; set; }

    [Required(ErrorMessage = "Name of table is required")]
    public string TableName { get; set; }
}