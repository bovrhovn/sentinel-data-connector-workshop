<#

# .SYNOPSIS

Install all required resources for CA

.DESCRIPTION
 
This script will install all required resources for CA demo from scratch
with the use of Bicep templates. It will also install Bicep and Azure CLI if selected and get environment 
variables from local.env file. 
  
.EXAMPLE

PS > Init-Environment -Location "WestEurope" -InstallModules -InstallBicep -UseEnvFile
install modules, bicep and deploy resources to West Europe

.EXAMPLE

PS > Init-Environment  -Location "WestEurope" 
install all required resources to West Europe
 
. LINK

http://github.com/bovrhovn
 
#>
param(
    [Parameter(Mandatory = $false)]
    $Location = "WestEurope",
    [Parameter(Mandatory = $false)]
    $EnvFileToReadFrom = "local.env",
    [Parameter(Mandatory = $false)]
    [switch]$InstallModules,
    [Parameter(Mandatory = $false)]
    [switch]$InstallBicep,
    [Parameter(Mandatory = $false)]
    [switch]$InstallAzCli,
    [Parameter(Mandatory = $false)]
    [switch]$UseEnvFile,   
    [Parameter(Mandatory = $false)]
    [switch]$LoginToAzure,
    [Parameter(Mandatory = $false)]
    $UserEmailToBeAddedToKeyVault
)

$ProgressPreference = 'SilentlyContinue';
Start-Transcript -Path "$HOME/Downloads/bootstrapper.log" -Force
# Write-Output "Sign in to Azure account." 
# login to Azure account

if ($LoginToAzure)
{
    Write-Output "Sign in to Azure account." 
    Connect-AzAccount
}

if ($InstallModules)
{
    Write-Output "Install Az module and register providers."
    #install Az module
    Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
    Install-Module -Name Az.App

    #register providers
    Register-AzResourceProvider -ProviderNamespace Microsoft.App
    # add support for log analytics
    Register-AzResourceProvider -ProviderNamespace Microsoft.OperationalInsights
    Write-Output "Modules installed and registered, continuing to Azure deployment nad if selected, Bicep install."
}

if ($InstallBicep)
{
    # install bicep
    Write-Output "Installing Bicep."
    # & Install-Bicep.ps1
    Start-Process powershell.exe -FilePath Install-Bicep.ps1 -NoNewWindow -Wait
    Write-Output "Bicep installed, continuing to Azure deployment."
}

if ($InstallAzCli)
{
    Write-Output "Installing Azure CLI."

    Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi;
    Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet';
    ## cleanup of Azure CLI
    Remove-Item .\AzureCLI.msi
    Write-Output "Az CLI installed, continuing to Azure deployment."
}
if ($UseEnvFile)
{
    Get-Content $EnvFileToReadFrom | ForEach-Object {
        $name, $value = $_.split('=')
        Set-Content env:\$name $value
        Write-Information "Writing $name to environment variable with $value."
    }
}

# create resource group if it doesn't exist with bicep file stored in bicep folder
$groupNameExport = New-AzSubscriptionDeployment -Location $Location -TemplateFile "bicep\rg.bicep" -TemplateParameterFile "bicep\rg.parameters.json" -Verbose
Write-Information $groupNameExport
$groupName = $groupNameExport.Outputs.rgName.Value
Write-Verbose "The resource group name is $groupName"
New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile "bicep\log-analytics.bicep" -TemplateParameterFile "Bicep\log-analytics.parameters.json" -Verbose
$lawName = $groupNameExport.Outputs.logAnalyticsId.Value
Write-Verbose "The log analytics workspace name is $lawName"
Write-Information "Creating keyvault"
$user = Get-AzADUser -ObjectId $UserEmailToBeAddedToKeyVault
$userId = $user.Id
Write-Verbose "The user id is $userId"
$kv = Get-Content "Bicep\keyvault.parameters.json" | ConvertFrom-Json
Write-Verbose "KV parameters $kv"
$kv.parameters.objectId.value = $userId
Write-Verbose "Overwriting KV parameters $kv"

$kv | ConvertTo-Json | Out-File "Bicep\keyvault.parameters.json" -Force

Write-Information "Deploying keyvault"
New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile "bicep\keyvault.bicep" -TemplateParameterFile "Bicep\keyvault.parameters.json" -Verbose



Stop-Transcript

# open file for viewing
Start-Process notepad.exe -ArgumentList "$HOME/Downloads/bootstrapper.log"
