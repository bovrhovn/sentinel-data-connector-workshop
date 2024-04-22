<#

# .SYNOPSIS

creates custom table in Log Analytics workspace

.DESCRIPTION
 
Creates custom table in Log Analytics workspace
  
.EXAMPLE

PS > .\Create-CustomTable.ps1 -ResourceGroupName "rg-ca" -TableName "MyTable" -WorkspaceName "MyWorkspace" -SubscriptionId "00000000-0000-0000-0000-000000000000"
create custom table in Log Analytics workspace
 
. LINK

https://learn.microsoft.com/en-us/azure/azure-monitor/agents/data-collection-text-log?tabs=portal
 
#>
param(
    [Parameter(Mandatory = $true,HelpMessage  = "Resource group name")]
    $ResourceGroupName = "rg-ca",
    [Parameter(Mandatory = $true,HelpMessage  = "Custom table name (without CL)")]
    $TableName,
    [Parameter(Mandatory = $true,HelpMessage  = "Workspace name")]
    $WorkspaceName
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
$subscription = $context.Subscription.Name
Write-Output "Subscription: $subscription in resource group $ResourceGroupName"
$subId= $context.Subscription.Id
Write-Output "Subscription ID: $subId"
$path="//subscriptions/$subId/resourcegroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$WorkspaceName/tables/#TABLENAME#_CL?api-version=2022-10-01"
$path = $path.Replace("#TABLENAME#", $TableName)
Write-Output "Path: $path"
Invoke-AzRestMethod -Path $path -Method PUT -payload $tableParams -ContentType "application/json" -UseBasicParsing
Write-Output "$TableName in $WorkspaceName in group $ResourceGroupName has been created"
