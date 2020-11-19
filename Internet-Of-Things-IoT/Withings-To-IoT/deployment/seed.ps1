#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param (
  [Parameter(Mandatory=$true)]
  [string]
  [ValidatePattern('^https://[^.]+\.azurehealthcareapis\.com$')]
  $FhirServerUrl,

  [Parameter(Mandatory=$true)]
  [string]
  [ValidateScript({ if (!(Test-Path $_ -PathType Container)) { throw 'Must be a directory' } else { return $true } })]
  $BundlesDirectory,

  [Parameter(Mandatory=$false)]
  [string]
  $FhirLoaderVersion = 'master',

  [Parameter(Mandatory=$false)]
  [string]
  $FhirLoaderDirectory = $(Join-Path $([System.IO.Path]::GetTempPath()) "FhirLoader-$FhirLoaderVersion")
)

if (!(Get-Command az -ErrorAction SilentlyContinue))
{
  throw 'Need to install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli'
}

if (!(Get-Command dotnet -ErrorAction SilentlyContinue))
{
  throw 'Need to install DotNet Core: https://docs.microsoft.com/en-us/dotnet/core/install/'
}

if (!(Test-Path -Path $FhirLoaderDirectory -PathType Container)) {
  Write-Output "Downloading FhirLoader to $FhirLoaderDirectory"
  $fhirLoaderZip = $FhirLoaderDirectory.TrimEnd('/\') + '.zip'
  (New-Object System.Net.WebClient).DownloadFile("https://github.com/hansenms/FhirLoader/archive/$FhirLoaderVersion.zip", $fhirLoaderZip)
  Expand-Archive -Path $fhirLoaderZip -DestinationPath "$FhirLoaderDirectory/.."
  Remove-Item $fhirLoaderZip
}

Write-Output "Getting access token for $FhirServerUrl"

$user = az account show --query 'user.name' --output tsv

$fhirResourceName = ($FhirServerUrl | Select-String -Pattern '^https://([^.]+)\.azurehealthcareapis\.com$').Matches.Groups[1].Value

$fhirResourceId = az resource list `
  --namespace Microsoft.HealthcareApis `
  --resource-type services `
  --name $fhirResourceName `
  --query '[0].id' `
  --output tsv

$roleAssignmentResourceId = az role assignment create `
  --assignee $user `
  --scope $fhirResourceId `
  --role 'FHIR Data Contributor' `
  --query 'id' `
  --output tsv

try
{
  $accessToken = az account get-access-token `
    --resource $FhirServerUrl `
    --query 'accessToken' `
    --output tsv

  dotnet run --project $FhirLoaderDirectory -- `
    --access-token $accessToken `
    --fhir-server-url $FhirServerUrl `
    --input-folder $BundlesDirectory
}
finally
{
  Write-Output 'Cleaning up'
  az role assignment delete `
    --id $roleAssignmentResourceId `
    --yes
}
