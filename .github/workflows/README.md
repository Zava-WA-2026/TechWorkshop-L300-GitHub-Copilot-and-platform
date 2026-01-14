# GitHub Actions Deployment Setup

## Required Secrets

### `AZURE_CREDENTIALS`
Create a service principal with Contributor access to your resource group:

```bash
az ad sp create-for-rbac --name "github-actions-sp" \
  --role Contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

Copy the entire JSON output and add it as a repository secret named `AZURE_CREDENTIALS`.

## Required Variables

Add these as repository variables in **Settings → Secrets and variables → Actions → Variables**:

| Variable | Description | Example |
|----------|-------------|---------|
| `ACR_NAME` | Azure Container Registry name | `zavaacrdevabcdef` |
| `ACR_LOGIN_SERVER` | ACR login server URL | `zavaacrdevabcdef.azurecr.io` |
| `WEBAPP_NAME` | App Service web app name | `zavawebdev` |

Get these values after deploying infrastructure:

```bash
az deployment group show -g {resource-group} -n main --query properties.outputs
```
