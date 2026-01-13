// foundry.bicep
// AI Foundry (Azure AI Foundry) deployment via official Azure Verified Module (AVM).
// Ref: br/public:avm/ptn/ai-ml/ai-foundry:0.6.0
param location string
param resourceNamePrefix string
param environment string

module aiFoundry 'br/public:avm/ptn/ai-ml/ai-foundry:0.6.0' = {
  params: {
    baseName: toLower('${resourceNamePrefix}${environment}')
    location: location
    includeAssociatedResources: false
    enableTelemetry: false
  }
}

output aiProjectName string = aiFoundry.outputs.aiProjectName
