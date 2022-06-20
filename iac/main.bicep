// -----------------------------------------------------------------------------
// Specific Parameters
// -----------------------------------------------------------------------------
@description('The resource location.')
param location string = resourceGroup().location

@description('The location for cosmos resources.')
param cosmosLocation string = location

@description('Connection string to sql database.')
param sqlConnection string

@description('App insights connection string.')
param appInsightsConnection string

@description('Source storage account connection string.')
param sourceStorageConnection string

@description('Target storage account connection string.')
param targetStorageConnection string

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

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------
module storageAccountDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/storage/storage-account:v1' = {
  name: 'storageAccountDeploy'
  params: {
    location: location
    prefix: prefix
    suffix: suffix
    tags: tags
  }
}

module storageBlobTriggerContainerDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/storage/storage-account-blob:v1' = {
  name: 'storageBlobTriggerContainerDeploy'
  params: {
    containerName: 'trigger'
    publicAccess: true
    storageAccountResourceName: storageAccountDeploy.outputs.resourceName
  }
}

module cosmosAccountDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/database/cosmos-account:v1' = {
  name: 'cosmosAccountDeploy'
  params: {
    isServerless: true
    isZoneRedundant: false
    useFreeTier: true
    location: cosmosLocation
    prefix: prefix
    suffix: suffix
    tags: tags
  }
}

module cosmosSqlDbDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/database/cosmos-sqldb:v1' = {
  name: 'cosmosSqlDbDeploy'
  params: {
    cosmosAccountResourceName: cosmosAccountDeploy.outputs.resourceName
    databaseName: 'data-extract-db'
    location: cosmosLocation
    tags: tags
  }
}

module cosmosSqlDbExtractionContainerDeploy 'br:devacrsharedweu.azurecr.io/bicep/modules/database/cosmos-sqldb-container:v1' = {
  name: 'cosmosSqlDbExtractionContainerDeploy'
  params: {
    containerName: 'extractions'
    cosmosSqlDbResourceName: '${cosmosAccountDeploy.outputs.resourceName}/${cosmosSqlDbDeploy.outputs.resourceName}'
    partitionKeys: [
      '/id'
    ]
    uniqueKeys: []
    location: cosmosLocation
    tags: tags
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
    appSettings: [
      {
        name: 'ApplicationInsights__ConnectionString'
        value: appInsightsConnection
      }
      {
        name: 'ConnectionStrings__SourceStorageAccount'
        value: sourceStorageConnection
      }
      {
        name: 'ConnectionStrings__TargetStorageAccount'
        value: targetStorageConnection
      }
      {
        name: 'ConnectionStrings__TriggerStorageAccount'
        value: storageAccountDeploy.outputs.primaryConnection
      }
      {
        name: 'ConnectionStrings__StateDatabase'
        value: cosmosAccountDeploy.outputs.cosmosConnection
      }
      {
        name: 'ConnectionStrings__SourceDatabase'
        value: sqlConnection
      }
    ]
    shortName: 'docextract'
    location: location
    prefix: prefix
    suffix: suffix
    tags: tags
  }
}
