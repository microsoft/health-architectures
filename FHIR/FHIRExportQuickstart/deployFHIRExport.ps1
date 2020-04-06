<#
.SYNOPSIS
    Deploy Logic App workflow for API for FHIR $export
.PARAMETER BatchComputeNodeRuntimeId
    Default: win10-x64
    Specify the dotnet runtime id in your compute node.
#>

param(
    [string]$Name = "fhirexport",
    [string]$Location = "eastus"
)

try {
    $rgName = $Name + 'rg'
    New-AzResourceGroup -Name $rgName -Location $Location
    New-AzResourceGroupDeployment -ResourceGroupName $rgName -TemplateFile "./arm_template.json" -TemplateParameterFile "./arm_template_parameters.json"
}
catch {
    Write-Host "Unable to deploy template. Please check parameters file or variables"
}