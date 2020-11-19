#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param (
  [Parameter(Mandatory=$true)]
  [string]
  [ValidatePattern('^[a-z][a-z0-9]{2,7}$')]
  $AppName,

  [Parameter(Mandatory=$true)]
  [string]
  [ValidateNotNullOrEmpty()]
  $WithingsClientId,

  [Parameter(Mandatory=$true)]
  [string]
  [ValidateNotNullOrEmpty()]
  $WithingsClientSecret,

  [Parameter(Mandatory=$false)]
  [string]
  $FirebaseServerKey = "",

  [Parameter(Mandatory=$false)]
  [string]
  $IosKeyId = "",

  [Parameter(Mandatory=$false)]
  [string]
  $IosBundleId = "",

  [Parameter(Mandatory=$false)]
  [string]
  $IosTeamId = "",

  [Parameter(Mandatory=$false)]
  [string]
  $IosPushToken = "",

  [Parameter(Mandatory=$true)]
  [string]
  [ValidatePattern('^[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}$')]
  $B2CClientId,

  [Parameter(Mandatory=$false)]
  [bool]
  $Retain = $false,

  [Parameter(Mandatory=$false)]
  [switch]
  $SaveLocalSettings = $false,

  [Parameter(Mandatory=$false)]
  [string]
  [ValidateNotNullOrEmpty()]
  $BuildId = $(Get-Random).ToString(),

  [Parameter(Mandatory=$false)]
  [string]
  [ValidateNotNullOrEmpty()]
  $B2CTenantName = 'h3dev',

  [Parameter(Mandatory=$false)]
  [string]
  [ValidatePattern('^[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}$')]
  $B2CTenantId = '4923f76a-4c9f-403c-8db1-8d75fb37e910',

  [Parameter(Mandatory=$false)]
  [string]
  [ValidateNotNullOrEmpty()]
  $B2CPolicyName = 'B2C_1_susi',

  [Parameter(Mandatory=$false)]
  [string]
  [ValidateNotNullOrEmpty()]
  $Location='SouthCentralUS'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $(Get-Item $PSCommandPath).Directory.parent.FullName

if (!(Get-Command az -ErrorAction SilentlyContinue))
{
  throw 'Need to install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli'
}

$azMinVersion = '2.11.1'
$azVersion = $(az version | ConvertFrom-Json).'azure-cli'

if ([version]$azVersion -lt [version]$azMinVersion)
{
  throw "Need to update Azure CLI to $azMinVersion or higher (current: $azVersion): https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
}

if (!(Get-Command npm -ErrorAction SilentlyContinue))
{
  throw 'Need to install NPM: https://www.npmjs.com/get-npm'
}

$npmMinVersion = '6.14'
$npmVersion = npm --version

if ([version]$npmVersion -lt [version]$npmMinVersion)
{
  throw "Need to update NPM to $npmMinVersion or higher (current: $npmVersion): https://www.npmjs.com/get-npm"
}

if (!(Get-Command dotnet -ErrorAction SilentlyContinue))
{
  throw 'Need to install DotNet Core: https://docs.microsoft.com/en-us/dotnet/core/install/'
}

$dotnetMinVersion = '3.1'
$dotnetVersion = dotnet --version

if ([version]$dotnetVersion -lt [version]$dotnetMinVersion)
{
  throw "Need to update DotNet Core to $dotnetMinVersion or higher (current: $dotnetVersion): https://docs.microsoft.com/en-us/dotnet/core/install/"
}

if (!(Get-Command databricks -ErrorAction SilentlyContinue))
{
  throw 'Need to install Databricks CLI: https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/'
}

$databricksMinVersion = '0.11.0'
$databricksVersion = $(databricks --version).Split(' ')[1]

if ([version]$databricksVersion -lt [version]$databricksMinVersion)
{
  throw "Need to update Databricks CLI to $databricksMinVersion or higher (current: $databricksVersion): https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/"
}

$python = $null
$pythonMinVersion = '3.6'

foreach ($maybePython in @('python3', 'python'))
{
  if ((Get-Command $maybePython -ErrorAction SilentlyContinue))
  {
    try
    {
      $pythonVersion = $(& $maybePython --version).Split(' ')[1]
    }
    catch
    {
      continue
    }

    if ([version]$pythonVersion -ge [version]$pythonMinVersion)
    {
      $python = $maybePython
      break
    }
  }
}

if (!$python)
{
  throw "Need to install Python $pythonMinVersion or higher: https://www.python.org/downloads/"
}

Import-Module $projectRoot/deployment/helpers.psm1

#region Resources

Push-Location $projectRoot/deployment

$resourceGroup = az group create `
  --name $AppName `
  --location $Location `
  --output json `
| ConvertFrom-Json

if ($Retain)
{
  az group update `
    --name $AppName `
    --tags autodelete=false `
  | Out-Null
}

Set-Content -Path ./azuredeploy.resources.parameters.json -Encoding ascii -Value @"
{
  "`$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "appName": { "value": "$AppName" },
    "withingsClientId": { "value": "$WithingsClientId" },
    "withingsClientSecret": { "value": "$WithingsClientSecret" },
    "firebaseServerKey": { "value": "$FirebaseServerKey" },
    "iosKeyId": { "value": "$IosKeyId" },
    "iosBundleId": { "value": "$IosBundleId" },
    "iosTeamId": { "value": "$IosTeamId" },
    "iosPushToken": { "value": "$IosPushToken" },
    "b2cClientId": { "value": "$B2CClientId" },
    "b2cPolicyName": { "value": "$B2CPolicyName" },
    "b2cTenantId": { "value": "$B2CTenantId" },
    "b2cTenantName": { "value": "$B2CTenantName" }
  }
}
"@

Set-Content -Path ./azuredeploy.resources.generated.json -Encoding ascii -Value $(ReadJsonc -Path ./azuredeploy.jsonc)

$deploymentName = 'resources'

$deployment = az deployment group create `
  --resource-group $resourceGroup.name `
  --template-file ./azuredeploy.resources.generated.json `
  --parameters '@./azuredeploy.resources.parameters.json' `
  --name $deploymentName `
  --output json `
| ConvertFrom-Json

if (!$deployment)
{
  throw 'ARM template deployment failed'
}

$portalStorageAccountResourceId = $deployment.properties.outputs.portalStorageAccountResourceId.value
$portalCdnEndpointHostName = $deployment.properties.outputs.portalCdnEndpointHostName.value
$apiSecretStoreName = $deployment.properties.outputs.apiSecretStoreName.value
$apiTokenStoreName = $deployment.properties.outputs.apiTokenStoreName.value
$apiFunctionAppName = $deployment.properties.outputs.apiFunctionAppName.value
$apiPrefix = $deployment.properties.outputs.apiPrefix.value
$apiManagementHostName = $deployment.properties.outputs.apiManagementHostName.value
$apiCacheName = $deployment.properties.outputs.apiCacheName.value
$apiDbName = $deployment.properties.outputs.apiDbName.value

$portalStorageAccountConnectionString = az storage account show-connection-string `
  --ids $portalStorageAccountResourceId `
  --query 'connectionString' `
  --output tsv

az storage blob service-properties update `
  --connection-string $portalStorageAccountConnectionString `
  --static-website `
  --index-document 'index.html' `
  --404-document '404.html'

$account = az account show --output json | ConvertFrom-Json

if ($account.user.type -eq 'servicePrincipal')
{
  az keyvault set-policy `
    --name $apiSecretStoreName `
    --spn $account.user.name `
    --secret-permissions get list set delete backup restore purge

  az keyvault set-policy `
    --name $apiTokenStoreName `
    --spn $account.user.name `
    --secret-permissions get list set delete backup restore purge
}
elseif ($account.user.type -eq 'user')
{
  az keyvault set-policy `
    --name $apiSecretStoreName `
    --upn $account.user.name `
    --secret-permissions get list set delete backup restore purge

  az keyvault set-policy `
    --name $apiTokenStoreName `
    --upn $account.user.name `
    --secret-permissions get list set delete backup restore purge
}

$apiFunctionOutboundIps = $(
  az functionapp show `
    --resource-group $resourceGroup.name `
    --name $apiFunctionAppName `
    --query 'outboundIpAddresses' `
    --output tsv
).Split(',')

if ($SaveLocalSettings)
{
  $myIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip" -UseBasicParsing).Content
  $apiFunctionOutboundIps += $myIp
}

az cosmosdb update `
  --resource-group $resourceGroup.name `
  --name $apiDbName `
  --ip-range-filter $($apiFunctionOutboundIps -join ',')

$apiCacheFirewallRules = az redis firewall-rules list `
  --resource-group $resourceGroup.name `
  --name $apiCacheName `
  --output json `
| ConvertFrom-Json

foreach ($apiCacheFirewallRule in $apiCacheFirewallRules)
{
  $keepRule = $false

  foreach ($apiFunctionOutboundIp in $apiFunctionOutboundIps)
  {
    if (($apiCacheFirewallRule.startIp -eq $apiFunctionOutboundIp) -and ($apiCacheFirewallRule.endIp -eq $apiFunctionOutboundIp))
    {
      $keepRule = $true
      break
    }
  }

  if (!$keepRule)
  {
    az redis firewall-rules delete `
      --resource-group $resourceGroup.name `
      --name $apiCacheName `
      --rule-name $apiCacheFirewallRule.name.Split('/')[1]
  }
}

for ($i = 0; $i -lt $apiFunctionOutboundIps.Length; $i++)
{
  $apiFunctionOutboundIp = $apiFunctionOutboundIps[$i]
  $addRule = $true

  foreach ($apiCacheFirewallRule in $apiCacheFirewallRules)
  {
    if (($apiCacheFirewallRule.startIp -eq $apiFunctionOutboundIp) -and ($apiCacheFirewallRule.endIp -eq $apiFunctionOutboundIp))
    {
      $addRule = $false
      break
    }
  }

  if ($addRule)
  {
    az redis firewall-rules create `
      --resource-group $resourceGroup.name `
      --name $apiCacheName `
      --rule-name "$apiFunctionAppName$BuildId$i" `
      --start-ip $apiFunctionOutboundIp `
      --end-ip $apiFunctionOutboundIp
  }
}

Pop-Location

#endregion

#region ML

& $projectRoot/deployment/deploy-ml.ps1 `
  -ResourceGroupName $resourceGroup.name `
  -DeploymentName $deploymentName `
  -Python $python `
  -SaveLocalSettings $SaveLocalSettings

#endregion

#region Backend

& $projectRoot/deployment/deploy-apim.ps1 `
  -ResourceGroupName $resourceGroup.name `
  -DeploymentName $deploymentName

& $projectRoot/deployment/deploy-api.ps1 `
  -ResourceGroupName $resourceGroup.name `
  -DeploymentName $deploymentName `
  -BuildId $BuildId `
  -SaveLocalSettings $SaveLocalSettings

#endregion

#region Portal

& $projectRoot/deployment/deploy-portal.ps1 `
  -ResourceGroupName $resourceGroup.name `
  -DeploymentName $deploymentName

#endregion

Write-Output "##vso[task.setvariable variable=API_ENDPOINT]https://$apiManagementHostName/$apiPrefix"

Write-Output '===================================================='
Write-Output 'Deployment done!'
Write-Output '----------------------------------------------------'
Write-Output 'Add the following to the B2C application reply URLs:'
Write-Output "    https://$portalCdnEndpointHostName/test.xhtml"
Write-Output "    https://$portalCdnEndpointHostName/b2c/auth"
Write-Output '----------------------------------------------------'
Write-Output 'Add the following to the Withings application reply URLs:'
Write-Output "    https://$portalCdnEndpointHostName/test.xhtml"
Write-Output "    https://$portalCdnEndpointHostName/withings/auth"
Write-Output "    https://$apiManagementHostName/$apiPrefix/withings/callback"
Write-Output '----------------------------------------------------'
Write-Output 'The H3 API spec is available at:'
Write-Output "    https://$apiManagementHostName/$apiPrefix/swagger.json"
Write-Output '----------------------------------------------------'
Write-Output 'The H3 Portal is available at:'
Write-Output "    https://$portalCdnEndpointHostName"
Write-Output '----------------------------------------------------'
Write-Output 'Have a wonderful day!'
Write-Output '===================================================='
