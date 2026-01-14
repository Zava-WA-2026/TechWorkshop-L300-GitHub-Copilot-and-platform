// cognitiveServicesRoleAssignment.bicep
// Assign Cognitive Services User role to App Service managed identity for AI Foundry access

param principalId string
param cognitiveServicesAccountName string

// Cognitive Services User role definition ID
var cognitiveServicesUserRoleId = 'a97b65f3-24c7-4388-baec-2e87135dc908'

resource cognitiveServices 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' existing = {
  name: cognitiveServicesAccountName
}

resource cognitiveServicesUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: cognitiveServices
  name: guid(cognitiveServices.id, principalId, 'CognitiveServicesUser')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesUserRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
