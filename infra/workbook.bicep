@description('The name of the workbook')
param workbookName string = 'AI Services Observability'

@description('The display name of the workbook')
param displayName string = 'AI Services Observability'

@description('The location for the workbook')
param location string = resourceGroup().location

@description('The resource ID of the Log Analytics workspace')
param workspaceResourceId string

@description('The serialized JSON definition of the workbook')
param serializedData string

resource workbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid(resourceGroup().id, workbookName)
  location: location
  kind: 'shared'
  properties: {
    displayName: displayName
    category: 'workbook'
    serializedData: serializedData
    sourceId: workspaceResourceId
  }
}

output workbookId string = workbook.id
output workbookName string = workbook.name
