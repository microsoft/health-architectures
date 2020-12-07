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
  $DeploymentName
)

$ErrorActionPreference = 'Stop'
$projectRoot = $(Get-Item $PSCommandPath).Directory.parent.FullName
Import-Module $projectRoot/deployment/helpers.psm1

#region Resources

Push-Location $projectRoot/deployment

$deployment = az deployment group show `
  --resource-group $ResourceGroupName `
  --name $DeploymentName `
  --output json `
| ConvertFrom-Json

$portalCdnEndpointHostName = $deployment.properties.outputs.portalCdnEndpointHostName.value
$portalCdnEndpointResourceId = $deployment.properties.outputs.portalCdnEndpointResourceId.value
$apiPrefix = $deployment.properties.outputs.apiPrefix.value
$apiManagementHostName = $deployment.properties.outputs.apiManagementHostName.value

#endregion

#region Portal

Push-Location $projectRoot/portal

if (!(Test-Path ./node_modules -PathType Container))
{
  npm ci
}

$env:NUNJUCKS_PORTAL_URL = "https://$portalCdnEndpointHostName"
$env:GATSBY_H3_API_BASE_PATH = "https://$apiManagementHostName/$apiPrefix"
$env:GATSBY_WITHINGS_REDIRECT_URL = "https://$portalCdnEndpointHostName/withings/auth"
$env:GATSBY_WITHINGS_CLIENT_ID = $withingsClientId
$env:GATSBY_B2C_REDIRECT = "https://$portalCdnEndpointHostName/b2c/auth"
$env:GATSBY_B2C_POLICY_SIGNUP_SIGNIN = $b2cPolicyName
$env:GATSBY_B2C_POLICY_FORGOT_PASSWORD = "B2C_1_reset"
$env:GATSBY_B2C_APP_ID = $B2CClientId
$env:GATSBY_B2C_TENANT_NAME = $B2CTenantName
npm run build

az storage blob upload-batch `
  --connection-string $portalStorageAccountConnectionString `
  --source ./public `
  --destination '$web' `
  --no-progress

az cdn endpoint purge `
  --content-paths '/*' `
  --ids $portalCdnEndpointResourceId `
  --no-wait

Pop-Location

#endregion
