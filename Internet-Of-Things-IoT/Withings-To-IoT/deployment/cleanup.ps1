#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'

if (!(Get-Command az -ErrorAction SilentlyContinue))
{
  throw 'Need to install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli'
}

if ($Args.Length -gt 0)
{
  $resourceGroups = $Args | ForEach-Object { az group show --name $_ --output json | ConvertFrom-Json }
}
else
{
  $resourceGroups = az group list --output json | ConvertFrom-Json
}

$deletedResourceGroups = @()

foreach ($resourceGroup in $resourceGroups)
{
  if ($resourceGroup.tags.autodelete -eq 'false')
  {
    Write-Output "Skipping explicitly retained resource group $($resourceGroup.name)"
    continue
  }

  if ($resourceGroup.name.EndsWith('ml-rg'))
  {
    Write-Output "Skipping Databricks resource group $($resourceGroup.name)"
    continue
  }

  Write-Output "Deleting $($resourceGroup.name)"
  az group delete --yes --no-wait --name $resourceGroup.name
  $deletedResourceGroups += $resourceGroup.name
}

foreach ($deletedResourceGroup in $deletedResourceGroups)
{
  do
  {
    $resourceGroupExists = az group exists --name $deletedResourceGroup --output json | ConvertFrom-Json

    if ($resourceGroupExists)
    {
      Write-Output "Waiting for deletion of $deletedResourceGroup"
      Start-Sleep -Seconds 30
    }
  }
  while ($resourceGroupExists)
}

$keyVaults = az keyvault list-deleted --output json | ConvertFrom-Json

foreach ($keyVault in $keyVaults)
{
  try
  {
    Write-Output "Purging KeyVault $($keyVault.name)"
    az keyvault purge --name $keyVault.name --location "$($keyVault.properties.location)"
  }
  catch
  {
    Write-Output "Unable to purge KeyVault $($keyVault.name)"
  }
}
