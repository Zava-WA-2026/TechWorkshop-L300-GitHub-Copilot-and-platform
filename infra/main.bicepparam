using './main.bicep'

// Default parameters for ZavaStorefront (Sweden Central)
param location = 'swedencentral'
param environment = 'dev'
param resourceNamePrefix = 'zava'

// Image in ACR (no registry prefix). Example: 'zavastorefront:1.0.0'
param containerImage = 'zavastorefront:latest'
