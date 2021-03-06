// -----------------------------------------------------------------------------
// Specific Parameters
// -----------------------------------------------------------------------------
@description('The resource location.')
param location string = resourceGroup().location

// @description('The location for cosmos resources.')
// param cosmosLocation string = location

@description('Connection string to sql database.')
param sqlConnection string

@description('App insights connection string.')
param appInsightsConnection string

@description('Source storage account name.')
param sourceDocsStorageAccountName string

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------
@description('The environment prefix.')
var prefix = split(resourceGroup().name, '-')[0]

@description('The workload name.')
var workload = split(resourceGroup().name, '-')[2]

@description('The short location suffix.')
var locationShort = split(resourceGroup().name, '-')[3]

@description('Amalgam of the workload and the (short) location name.')
var suffix = '${workload}-${locationShort}'

@description('The resource tags.')
var tags = resourceGroup().tags

@description('The export container name.')
var exportBlobContainerName = 'wns-data-extract-export'

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------
module extractorStorageAccountDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/storage/storage-account:v1' = {
  name: 'extractorStorageAccountDeploy'
  params: {
    location: location
    prefix: prefix
    suffix: suffix
    tags: tags
  }
}

module extractorTriggerContainerDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/storage/storage-account-blob:v1' = {
  name: 'extractorTriggerContainerDeploy'
  params: {
    containerName: 'wns-data-extract-trigger'
    publicAccess: false
    storageAccountResourceName: extractorStorageAccountDeploy.outputs.resourceName
  }
}

module extractorExportContainerDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/storage/storage-account-blob:v1' = {
  name: 'extractorExportContainerDeploy'
  params: {
    containerName: exportBlobContainerName
    publicAccess: false
    storageAccountResourceName: extractorStorageAccountDeploy.outputs.resourceName
  }
}

module appServicePlanDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/web/app-service-plan:v1' = {
  name: 'appServicePlanDeploy'
  params: {  
    location: location
    prefix: prefix
    suffix: suffix
    tags: tags   
  }
}

module functionAppDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/web/app-service:v1' = {
  name: 'functionAppDeploy'
  params: {
    appServicePlanId: appServicePlanDeploy.outputs.resourceId
    isFunctionApp: true
    shortName: 'func'
    appSettings: [
      {
        name: 'AZURE_FUNCTIONS_ENVIRONMENT'
        value: 'prod'
      }
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'prod'
      }
      {
        name: 'DOTNET_ENVIRONMENT'
        value: 'prod'
      }
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        value: appInsightsConnection
      }
      {
        name: 'FUNCTIONS_EXTENSION_VERSION'
        value: '~4'
      }
      {
        name: 'FUNCTIONS_WORKER_RUNTIME'
        value: 'dotnet'
      }
      {
        name: 'WEBSITE_RUN_FROM_PACKAGE'
        value: 1
      }
      {
        name: 'ConnectionStrings__SourceDb'
        value: sqlConnection
      }
      {
        name: 'AzureWebJobsStorage'
        value: extractorStorageAccountDeploy.outputs.primaryConnection
      }
      {
        name: 'SourceDocsStorageAccountName'
        value: sourceDocsStorageAccountName
      }
      {
        name: 'ExportBlobContainerName'
        value: exportBlobContainerName
      }
      {
        name: 'ExportBlobStorage__accountName'
        value: extractorStorageAccountDeploy.outputs.resourceName
      }
    ]
    location: location
    prefix: prefix
    suffix: suffix
    tags: tags
  }
}

// module functionAppBlobContributorRoleDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/security/sp-assign-sub-role:v1' = {
//   name: 'functionAppBlobContributorRoleDeploy'
//   scope: subscription()
//   params: {
//     principalId: functionAppDeploy.outputs.resourcePrincipalId
//     role: 'Storage Blob Data Contributor'
//   }
// }
