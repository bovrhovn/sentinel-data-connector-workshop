Pwsh script to use to add logs to Sentinel:
https://www.powershellgallery.com/packages/Upload-AzMonitorLog/1.2

The Upload-AzMonitorLog PowerShell script enables you to use PowerShell to stream events or context information to
Microsoft Sentinel from the command line.

`` PowerShell

Import-Csv .\testcsv.csv
| .\Upload-AzMonitorLog.ps1
-WorkspaceId $WSId
-WorkspaceKey $WSKey
-LogTypeName 'MyNewCSV'
-AddComputerName
-AdditionalDataTaggingName "Environment"
-AdditionalDataTaggingValue "Demo"

``
