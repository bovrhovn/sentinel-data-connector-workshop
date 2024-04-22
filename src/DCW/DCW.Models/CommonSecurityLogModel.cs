namespace DCW.Models;

public class CommonSecurityLogModel
{
    public string Activity { get; set; }
    public string Computer { get; set; }
    public string DestinationIp { get; set; }
    public string SourceSystem { get; set; }
    public string Message { get; set; }
}