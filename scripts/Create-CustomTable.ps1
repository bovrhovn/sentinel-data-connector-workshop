<#

# .SYNOPSIS

creates custom table in Log Analytics workspace with the currently signed in user

.DESCRIPTION
 
Creates custom table in Log Analytics workspace
  
.EXAMPLE

PS > .\Create-CustomTable.ps1 -ResourceGroupName "rg-sdc" -TableName "MyTable" -WorkspaceName "MyWorkspace"
create custom table in Log Analytics workspace MyWorkspace in resource group rg-sdc with the name MyTable
 
. LINK

https://learn.microsoft.com/en-us/azure/azure-monitor/agents/data-collection-text-log?tabs=portal
 
#>
param(
    [Parameter(HelpMessage  = "Resource group name")]
    $ResourceGroupName = "rg-sdc",
    [Parameter(HelpMessage  = "Custom table name (without CL)")]
    $TableName = "MyTable",
    [Parameter(HelpMessage  = "Workspace name")]
    $WorkspaceName = "law-sdc"
)
Write-Output "Creating custom table $TableName in $WorkspaceName in group $ResourceGroupName"
$tableParams = @'
{
   "properties": {
       "schema": {
              "name": "#TABLE#_CL",
              "tableType": "CustomLog",
              "retentionInDays": 45,
              "totalRetentionInDays": 70,
              "columns": [
                   {
                      "name": "TimeGenerated",
                      "type": "datetime"
                    },
                    {
                      "name": "DateCreated",
                      "type": "datetime"
                    },
                    {
                      "name": "SourceIP",
                      "type": "string"
                    },
                    {
                      "name": "DestinationIP",
                      "type": "string"
                    },
                    {
                      "name": "Message",
                      "type": "string"
                    },
                    {
                      "name": "EventType",
                      "type": "int"
                    },
                    {
                      "name": "AuditEventId",
                      "type": "string"
                    }
             ]
       }
   }
}
'@

$tableParams = $tableParams.Replace("#TABLE#", $TableName)
Write-Output "Table params: $tableParams"
$context = Get-AzContext
Write-Verbose "Context: $context"
$subscription = $context.Subscription.Name
Write-Output "Subscription: $subscription in resource group $ResourceGroupName"
$subId= $context.Subscription.Id
Write-Output "Subscription ID: $subId"
$path="//subscriptions/$subId/resourcegroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$WorkspaceName/tables/#TABLENAME#_CL?api-version=2022-10-01"
$path = $path.Replace("#TABLENAME#", $TableName)
Write-Output "Path: $path"
try
{
    Invoke-AzRestMethod -Path $path -Method PUT -payload $tableParams -Verbose
    Write-Output "$TableName in $WorkspaceName in group $ResourceGroupName has been created"
}
catch
{
    Write-Output "Failed to create $TableName in $WorkspaceName in group $ResourceGroupName"
    Write-Output $_.Exception.Message
}