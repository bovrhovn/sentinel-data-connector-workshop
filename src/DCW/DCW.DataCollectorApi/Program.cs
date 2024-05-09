using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;

//LINK: https://learn.microsoft.com/en-us/azure/azure-monitor/logs/data-collector-api?tabs=powershell

var json =
    @"[{""DemoField1"":""DemoValue1"",""DemoField2"":""DemoValue2""},{""DemoField3"":""DemoValue3"",""DemoField4"":""DemoValue4""}]";

AnsiConsole.Write(new FigletText("Use DataCollector API").Centered().Color(Color.Red));

// Update customerId to your Log Analytics workspace ID
var customerId = Environment.GetEnvironmentVariable("WORKSPACEID");
if (string.IsNullOrEmpty(customerId))
{
    AnsiConsole.Write(new Markup("[red]WORKSPACEID[/] is not set in the environment variables"));
    AnsiConsole.WriteLine();
    return;
}
// For sharedKey, use either the primary or the secondary Connected Sources client authentication key   
var sharedKey = Environment.GetEnvironmentVariable("SECRETKEY");
if (string.IsNullOrEmpty(sharedKey))
{
    AnsiConsole.Write(new Markup("[red]WORKSPACEID[/] is not set in the environment variables"));
    AnsiConsole.WriteLine();
    return;
}

// LogName is name of the event type that is being submitted to Azure Monitor
var tableName = "DemoCato";

// Create a hash for the API signature
var datestring = DateTime.UtcNow.ToString("r");
var jsonBytes = Encoding.UTF8.GetBytes(json);
var stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
var hashedString = BuildSignature(stringToHash, sharedKey);
var signature = "SharedKey " + customerId + ":" + hashedString;

//post the data
await PostData(signature, datestring, json, tableName, string.Empty, customerId);

AnsiConsole.WriteLine("Press any key to exit");

// Build the API signature
string BuildSignature(string message, string secret)
{
    var encoding = new ASCIIEncoding();
    var keyByte = Convert.FromBase64String(secret);
    var messageBytes = encoding.GetBytes(message);
    using var hmacsha256 = new HMACSHA256(keyByte);
    var hash = hmacsha256.ComputeHash(messageBytes);
    return Convert.ToBase64String(hash);
}

// Send a request to the POST API endpoint
async Task PostData(string signature, string date, string json, string logName, string timeStampField,
    string customerId)
{
    try
    {
        var url = "https://" + customerId + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Log-Type", logName);
        client.DefaultRequestHeaders.Add("Authorization", signature);
        client.DefaultRequestHeaders.Add("x-ms-date", date);
        client.DefaultRequestHeaders.Add("time-generated-field", timeStampField);

        // If charset=utf-8 is part of the content-type header, the API call may return forbidden.
        var httpContent = new StringContent(json, Encoding.UTF8);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(new Uri(url), httpContent);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content;
            var result = await responseContent.ReadAsStringAsync();
            AnsiConsole.WriteLine("Return Result: " + result);
        }
        else
        {
            AnsiConsole.WriteLine("Failed to post data to Azure Monitor");
        }
    }
    catch (Exception excep)
    {
        AnsiConsole.WriteException(excep);
    }
}