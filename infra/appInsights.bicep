// appInsights.bicep
// Application Insights module
param location string
param resourceNamePrefix string
param environment string

resource appi 'Microsoft.Insights/components@2020-02-02' = {
  name: '${resourceNamePrefix}appi${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output instrumentationKey string = appi.properties.InstrumentationKey
output connectionString string = appi.properties.ConnectionString
output appInsightsName string = appi.name
