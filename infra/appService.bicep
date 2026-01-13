// appService.bicep
// Linux App Service (Web App for Containers) module
param location string
param resourceNamePrefix string
param environment string
param acrLoginServer string
param appInsightsConnectionString string

@description('Container image repo:tag stored in ACR (no registry prefix).')
param containerImage string = 'zavastorefront:latest'

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: '${resourceNamePrefix}plan${environment}'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: '${resourceNamePrefix}web${environment}'
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/${containerImage}'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: ''
      alwaysOn: true
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acrLoginServer}'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output principalId string = webApp.identity.principalId
output webAppName string = webApp.name
