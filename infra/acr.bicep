// acr.bicep
// Azure Container Registry module
param location string
param resourceNamePrefix string
param environment string

var uniqueSuffix = substring(uniqueString(subscription().id, resourceGroup().id, resourceNamePrefix, environment), 0, 6)
// ACR name: 5-50 chars, alphanumeric only, globally unique.
var acrName = toLower('${resourceNamePrefix}acr${environment}${uniqueSuffix}')
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

output loginServer string = acr.properties.loginServer
output acrResourceId string = acr.id
output acrName string = acr.name
