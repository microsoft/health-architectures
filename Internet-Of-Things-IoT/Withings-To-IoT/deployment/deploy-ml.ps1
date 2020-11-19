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

  [Parameter(Mandatory=$false)]
  [string]
  $Python = 'python',

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

$mlDatabricksWorkspaceResourceId = $deployment.properties.outputs.mlDatabricksWorkspaceResourceId.value
$mlDatabricksWorkspaceHostName = $deployment.properties.outputs.mlDatabricksWorkspaceHostName.value
$mlCaptureDataLakeName = $deployment.properties.outputs.mlCaptureDataLakeName.value
$apiSecretStoreName = $deployment.properties.outputs.apiSecretStoreName.value
$apiFunctionAppName = $deployment.properties.outputs.apiFunctionAppName.value

#endregion

#region ML

Push-Location $projectRoot/ml

$databricksTokenSecretName = "$($mlDatabricksWorkspaceResourceId.Split('/')[-1])token"

$ErrorActionPreference = 'SilentlyContinue'
$databricksTokenSecret = az keyvault secret show `
  --vault-name $apiSecretStoreName `
  --name $databricksTokenSecretName `
  --output json `
2> $null | ConvertFrom-Json
$ErrorActionPreference = 'Stop'

if (!$databricksTokenSecret)
{
  $databricksToken = az account get-access-token `
    --resource 2ff814a6-3304-4ab8-85cb-cd0e6f879c1d `
    --query accessToken `
    --output tsv

  $armToken = az account get-access-token `
    --resource https://management.core.windows.net/ `
    --query accessToken `
    --output tsv

  $createTokenResponse = Invoke-WebRequest `
    -Uri "https://$mlDatabricksWorkspaceHostName/api/2.0/token/create" `
    -Body $(@{ "comment" = $AppName; } | ConvertTo-Json) `
    -Method POST `
    -Headers @{
      "Accept" = "application/json";
      "Authorization" = "Bearer $databricksToken";
      "X-Databricks-Azure-SP-Management-Token" = $armToken;
      "X-Databricks-Azure-Workspace-Resource-Id" = $mlDatabricksWorkspaceResourceId;
    } `
    -UseBasicParsing `
  | ConvertFrom-Json

  $databricksTokenSecret = az keyvault secret set `
    --vault-name $apiSecretStoreName `
    --name $databricksTokenSecretName `
    --value $createTokenResponse.token_value `
    --output json `
  | ConvertFrom-Json
}

$env:DATABRICKS_HOST = "https://$mlDatabricksWorkspaceHostName"
$env:DATABRICKS_TOKEN = $databricksTokenSecret.value

if ($SaveLocalSettings)
{
  Set-Content -Path ~/.databrickscfg -Value @(
    "[DEFAULT]",
    "host = $env:DATABRICKS_HOST",
    "token = $env:DATABRICKS_TOKEN"
  )
}

# TODO: add a second databricks token that is scoped down to only call the API instead of using the deployment token

$databricksTokenSecretReference = '"@Microsoft.KeyVault(SecretUri={0})"' -f $databricksTokenSecret.id

$h3UtilsVersion = Get-Content -Encoding 'UTF8' ./version.txt
& $Python ./setup.py bdist_wheel
databricks fs cp --overwrite "./dist/h3_utils-$h3UtilsVersion-py2.py3-none-any.whl" "dbfs:/libs/h3_utils-$h3UtilsVersion-py2.py3-none-any.whl"

databricks workspace import_dir --overwrite --exclude-hidden-files ./jobs /Shared/ml/jobs

# TODO: replace the scope management with a KeyVault backed scope once the feature is available in the CLI or API

$scopeNames = $(databricks secrets list-scopes --output json | ConvertFrom-Json).scopes | ForEach-Object { $_.name }

if (!($scopeNames -contains 'storage'))
{
  databricks secrets create-scope --scope storage --initial-manage-principal users
}

foreach ($storageAccountName in @(
  $mlCaptureDataLakeName
))
{
  $storageAccessKey = az storage account keys list `
    --account-name $storageAccountName `
    --query '[0].value' `
    --output tsv

  databricks secrets put `
    --scope storage `
    --key "$($storageAccountName)AccessKey" `
    --string-value $storageAccessKey
}

foreach ($jobTemplatePath in $(Get-ChildItem -Path . -Recurse -Filter '*.job.jsonc'))
{
  $jobTemplate = Templatize -Text $(ReadJsonc -Path $jobTemplatePath.FullName) -Deployment $deployment.properties.outputs.PSObject
  $jobTemplate = $jobTemplate.Replace('{{ h3UtilsVersion }}', $h3UtilsVersion)

  $jobJsonPath = "$($jobTemplatePath.FullName).created"
  $jobTemplate | Set-Content -Path $jobJsonPath
  $job = $jobTemplate | ConvertFrom-Json
  $jobId = $null
  $deleteUserJobId = $null

  foreach ($existingJob in $(databricks jobs list --output json | ConvertFrom-Json).jobs)
  {
    if ($existingJob.settings.name -eq $job.name)
    {
      $jobId = $existingJob.job_id
      break
    }
  }

  if ($jobId)
  {
    databricks jobs reset --json-file $jobJsonPath --job-id $jobId
  }
  else
  {
    $jobId = $(databricks jobs create --json-file $jobJsonPath | ConvertFrom-Json).job_id
  }

  if ($job.name -eq 'delete_user')
  {
    $deleteUserJobId = $jobId
  }
}

az functionapp config appsettings set `
  --resource-group $resourceGroup.name `
  --name $apiFunctionAppName `
  --settings `
      DATABRICKS_DELETE_USER_JOB_ID=$deleteUserJobId `
      DATABRICKS_HOST=$env:DATABRICKS_HOST `
      DATABRICKS_TOKEN=$databricksTokenSecretReference

Pop-Location

#endregion
