// main.bicep
// Entry point for ZavaStorefront infra deployment in Sweden Central
// Modules: ACR, App Service, App Insights, Foundry, Role Assignment

param location string = 'swedencentral'
param environment string = 'dev'
param resourceNamePrefix string = 'zava'

@description('Container image repo:tag stored in ACR (no registry prefix).')
param containerImage string = 'zavastorefront:latest'

module acr 'acr.bicep' = {
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    environment: environment
  }
}

module appService 'appService.bicep' = {
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    environment: environment
    acrLoginServer: acr.outputs.loginServer
    appInsightsConnectionString: appInsights.outputs.connectionString
    containerImage: containerImage
  }
}

module appInsights 'appInsights.bicep' = {
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    environment: environment
  }
}

module foundry 'foundry.bicep' = {
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    environment: environment
  }
}

module roleAssignment 'roleAssignment.bicep' = {
  params: {
    principalId: appService.outputs.principalId
    acrName: acr.outputs.acrName
  }
}

output acrName string = acr.outputs.acrName
output acrLoginServer string = acr.outputs.loginServer
output webAppName string = appService.outputs.webAppName
output appInsightsConnectionString string = appInsights.outputs.connectionString
output aiProjectName string = foundry.outputs.aiProjectName
