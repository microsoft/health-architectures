#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param (
  [Parameter(Mandatory=$true)]
  [string]
  [ValidateNotNullOrEmpty()]
  $ResourceGroupName,

  [Parameter(Mandatory=$true)]
  [string]
  [ValidateNotNullOrEmpty()]
  $DeploymentName,

  [Parameter(Mandatory=$true)]
  [string]
  [ValidateNotNullOrEmpty()]
  $BuildId,

  [Parameter(Mandatory=$false)]
  [bool]
  $SaveLocalSettings = $false
)

$ErrorActionPreference = 'Stop'
$projectRoot = $(Get-Item $PSCommandPath).Directory.parent.FullName
Import-Module $projectRoot/deployment/helpers.psm1

#region Resources

$deployment = az deployment group show `
  --resource-group $ResourceGroupName `
  --name $DeploymentName `
  --output json `
| ConvertFrom-Json

$apiFunctionAppName = $deployment.properties.outputs.apiFunctionAppName.value
$apiFunctionStorageAccountResourceId = $deployment.properties.outputs.apiFunctionStorageAccountResourceId.value

#endregion

#region API

Push-Location $projectRoot/backend/api

dotnet publish --configuration Release

Compress-Archive `
  -Path ./bin/Release/netcoreapp3.1/* `
  -DestinationPath ./zipdeploy.zip `
  -Force

$apiFunctionStorageAccountConnectionString = az storage account show-connection-string `
  --ids $apiFunctionStorageAccountResourceId `
  --query 'connectionString' `
  --output tsv

$codeContainerName = 'code'

az storage container create `
  --connection-string $apiFunctionStorageAccountConnectionString `
  --name $codeContainerName `
  --public-access off

$codeBlobName = "$BuildId.zip"

az storage blob upload `
  --connection-string $apiFunctionStorageAccountConnectionString `
  --container-name $codeContainerName `
  --name $codeBlobName `
  --file ./zipdeploy.zip `
  --no-progress

$codeBlobUrl = az storage blob generate-sas `
  --connection-string $apiFunctionStorageAccountConnectionString `
  --container-name $codeContainerName `
  --name $codeBlobName `
  --permissions r `
  --start '2020-01-01T00:00Z' `
  --expiry '2222-01-01T00:00Z' `
  --https-only `
  --full-uri `
  --output tsv

$codeBlobUrlSetting = '"{0}"' -f $codeBlobUrl

az functionapp config appsettings set `
  --resource-group $ResourceGroupName `
  --name $apiFunctionAppName `
  --settings `
      WEBSITE_RUN_FROM_PACKAGE=$codeBlobUrlSetting

while ($true)
{
  az resource invoke-action `
    --resource-group $ResourceGroupName `
    --resource-type 'Microsoft.Web/sites' `
    --name $apiFunctionAppName `
    --api-version '2016-08-01' `
    --action 'syncfunctiontriggers'

  if ($? -eq $true)
  {
    break
  }

  Start-Sleep -Seconds 5
}

if ($SaveLocalSettings)
{
  $apiFunctionAppSettings = az functionapp config appsettings list `
    --resource-group $ResourceGroupName `
    --name $apiFunctionAppName `
    --output json `
  | ConvertFrom-Json

  $localSettings = @{
    IsEncrypted = $false;
    Values = @{};
    Host = @{
      LocalHttpPort = 8888;
      CORS = "*";
      CORSCredentials = $false;
    };
    ConnectionStrings = @{};
  }

  foreach ($setting in $apiFunctionAppSettings)
  {
    try
    {
      $secretUri = ($setting.value | Select-String -Pattern '^@Microsoft.KeyVault\(SecretUri=([^)]+)\)$').Matches.Groups[1].Value
      $secret = az keyvault secret show --id $secretUri --output json | ConvertFrom-Json
      $localSettings.Values[$setting.name] = $secret.value
    }
    catch
    {
      $localSettings.Values[$setting.name] = $setting.value
    }
  }

  $localSettings | ConvertTo-Json | Set-Content ./local.settings.json

  $roleAssignee = az account show --query 'user.name' --output tsv
  $roleScope = az group show --name $ResourceGroupName --query 'id' --output tsv

  foreach ($role in @(
    'Azure Event Hubs Data Owner',
    'FHIR Data Contributor'
  ))
  {
    az role assignment create `
      --assignee $roleAssignee `
      --scope $roleScope `
      --role $role
  }
}

Pop-Location

#endregion
