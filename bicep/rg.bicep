targetScope = 'subscription'
@description('Resource Group Name')
param resourceGroupName string = 'rg-ca'

@description('Resource Group Location')
param resourceGroupLocation string = 'WestEurope'

param resourceTags object = {
  Description: 'Resource group for CA demo'
  Environment: 'Demo'
  ResourceType: 'ResourceGroup'
}

resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName 
  tags: resourceTags 
  location: resourceGroupLocation
}

@description('Output resource group name')
output rgName string = resourceGroupName
