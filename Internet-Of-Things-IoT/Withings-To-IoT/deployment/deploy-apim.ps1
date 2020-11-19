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

$deployment = az deployment group show `
  --resource-group $ResourceGroupName `
  --name $DeploymentName `
  --output json `
| ConvertFrom-Json

$apiFunctionAppName = $deployment.properties.outputs.apiFunctionAppName.value
$apiPrefix = $deployment.properties.outputs.apiPrefix.value
$apiHostName = $deployment.properties.outputs.apiHostName.value
$apiManagementName = $deployment.properties.outputs.apiManagementName.value
$apiManagementApiName = $deployment.properties.outputs.apiManagementApiName.value

#endregion

#region APIM

Push-Location $projectRoot/backend/cli

dotnet run -- swagger ./swagger.json

# TODO: find a way to create the api resource without having to delete and re-create it

$apis = az apim api list `
  --only-show-errors `
  --resource-group $ResourceGroupName `
  --service-name $apiManagementName `
  --output json `
| ConvertFrom-Json

foreach ($api in $apis)
{
  az apim api delete `
    --only-show-errors `
    --resource-group $ResourceGroupName `
    --service-name $apiManagementName `
    --api-id $api.name `
    --yes
}

az apim api import `
  --only-show-errors `
  --resource-group $ResourceGroupName `
  --service-name $apiManagementName `
  --api-id $apiManagementApiName `
  --api-type http `
  --specification-format Swagger `
  --specification-path ./swagger.json `
  --path $apiPrefix `
  --display-name $apiFunctionAppName `
  --service-url "https://$apiHostName" `
  --protocols https `
  --subscription-required false

Pop-Location

Push-Location $projectRoot/deployment

$policiesDeploymentTemplate = ConvertFrom-Json '
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {},
  "variables": {},
  "resources": [],
  "outputs": {}
}'

foreach ($policy in $(Get-ChildItem ./policies/endpoints -Filter '*.xml'))
{
  $policiesDeploymentTemplate.resources += @{
    name = "$apiManagementName/$apiManagementApiName/$($policy.BaseName)/policy";
    type = 'Microsoft.ApiManagement/service/apis/operations/policies';
    apiVersion = '2019-12-01';
    properties = @{
      format = 'xml';
      value = $(Templatize -Text $(ReadXml -Path $policy.FullName) -Deployment $deployment.properties.outputs.PSObject);
    }
  }
}

$policiesDeploymentTemplate.resources += @{
  name = "$apiManagementName/$apiManagementApiName/policy";
  type = 'Microsoft.ApiManagement/service/apis/policies';
  apiVersion = '2019-12-01';
  properties = @{
    format = 'xml';
    value = $(Templatize -Text $(ReadXml -Path ./policies/api.xml) -Deployment $deployment.properties.outputs.PSObject);
  }
}

$policiesDeploymentTemplate | ConvertTo-Json -Depth 10 | Set-Content -Path ./policies/azuredeploy.policies.generated.json -Encoding ascii

$policiesDeploymentName = 'policies'

$policiesDeployment = az deployment group create `
  --resource-group $ResourceGroupName `
  --template-file ./policies/azuredeploy.policies.generated.json `
  --name $policiesDeploymentName `
  --output json `
| ConvertFrom-Json

if (!$policiesDeployment)
{
  throw 'ARM policies deployment failed'
}

Pop-Location

#endregion
