using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.OperationalInsights;
using Microsoft.Azure.Management.OperationalInsights.Models;
using Microsoft.Rest;
using Spectre.Console;

#nullable disable

AnsiConsole.Write(new FigletText("Generate table").Centered().Color(Color.Red));

# region Environment variables

var subscriptionId = Environment.GetEnvironmentVariable("SubscriptionId") ?? string.Empty;
if (subscriptionId == string.Empty)
{
    AnsiConsole.Write(new Markup("[red]SubscriptionId[/] is not set in the environment variables"));
    AnsiConsole.WriteLine();
    return;
}
var resourceGroupName = Environment.GetEnvironmentVariable("ResourceGroupName") ?? string.Empty;
if (resourceGroupName == string.Empty)
{
    AnsiConsole.Write(new Markup("[red]ResourceGroupName[/] is not set in the environment variables"));
    AnsiConsole.WriteLine();
    return;
}
var workspaceName = Environment.GetEnvironmentVariable("WorkspaceName") ?? string.Empty;
if (workspaceName == string.Empty)
{
    AnsiConsole.Write(new Markup("[red]WorkspaceName[/] is not set in the environment variables"));
    AnsiConsole.WriteLine();
    return;
}

#endregion

var tableName = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]TABLE name[/] (add _CL on the end)?")
        .PromptStyle("red"));

AnsiConsole.Write(new Markup(
    $"Starting operation in [bold yellow]{subscriptionId}[/] in RG [red]{resourceGroupName}[/] to workspace [red]{workspaceName}[/] with table [red]{tableName}[/] "));
AnsiConsole.WriteLine();

var tokenCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
});

var accessToken = await tokenCredentials.GetTokenAsync(new TokenRequestContext([
    "https://management.core.windows.net/.default"
]));

var restTokenCredentials = new TokenCredentials(accessToken.Token);
var managementClient = new OperationalInsightsManagementClient(restTokenCredentials)
{
    SubscriptionId = subscriptionId
};

AnsiConsole.Write(new Markup("Logged in. Starting to [bold]add table[/] - wait a minute..."));
AnsiConsole.WriteLine();
var table = new Microsoft.Azure.Management.OperationalInsights.Models.Table
{
    RetentionInDays = 30,
    Schema = new Schema
    {
        Name = tableName,
        Columns = new List<Column>
        {
            new Column
            {
                Name = "TimeGenerated",
                Type = "dateTime"
            }
            , new Column
            {
                Name = "DateCreated",
                Type = "dateTime"
            },
            new Column
            {
                Name = "SourceIP",
                Type = "string"
            },
            new Column
            {
                Name = "DestinationIP",
                Type = "string"
            },
            new Column
            {
                Name = "Message",
                Type = "string"
            },
            new Column
            {
                Name = "EventType",
                Type = "int"
            },
            new Column
            {
                Name = "AuditEventId",
                Type = "string"
            }
        }
    }
};

try
{
    var response =
        await managementClient.Tables.CreateOrUpdateAsync(resourceGroupName, workspaceName, tableName, table);
    if (response == null)
        AnsiConsole.WriteLine("We didn't add ");
}
catch (Exception e)
{
    AnsiConsole.WriteException(e);
}

AnsiConsole.WriteLine("Done with execution.... exiting....");