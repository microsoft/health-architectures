<#
.AUTHOR Cory Stevenson, Microsoft
.EDITOR Benjamin Xue, Microsoft
Repo: https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRExportwithAnonymiztion 
Parent Repo: https://github.com/microsoft/health-architectures 
.SYNOPSIS
    Deploy an end to end solution for setting up a research environment with safe houbor rules applied to the data. 
    Prerequisite:
    - API for FHIR
    - Integration setup for 
    Core components:
    - Logic App workflow for API for FHIR $export
    - Data anonymization reqruied services including Data Factory and Batch
    - Key Vault to store service account credentials
    - Storage accounts, one for storing anonymized data and one for storing ADF and custom activity files
    Pre Deployment Configuration:
    - Modify the FHIR url, service principal client id, secret, and tenant id in the arm_template.parameters.json file prior to deployment
    <FIX THIS>
    - Modify FHIR data anonymization rules if necessary post deployment. The deployment supports the safe harbor method. The configuration file will be located in the 
    - A list of FHIR data anonymization configutations are available here:  https://github.com/microsoft/FHIR-Tools-for-Anonymization#configuration-file-format
.PARAMETER BatchComputeNodeRuntimeId
    Default: win10-x64
    Specify the dotnet runtime id in your compute node.
#>

param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(5,12)]
    [ValidateScript({
        if (("$_" -cmatch "(^([a-z]|\d)+$)") -and ($_.Substring(0,1) -notmatch "^[0-9]"))
        {
            return $true
        }
        else
        {
			Write-Host "Vault name must only contain alphanumeric characters and dashes and cannot start with a number.
Vault name must be between 3-24 alphanumeric characters. The name must begin with a letter, end with a letter or digit, and not contain hyphens." -ForegroundColor "Yellow"
            return $false
        }
    })]
    [string]$EnvironmentName,

    [Parameter(Mandatory = $false)]
    [string]$EnvironmentLocation = "eastus2",

    # If the FHIR integration has already been setup with a storage account, then add the name of the storage account as a paramenter input. Otherwise a new storage account will be created.
    [Parameter(Mandatory = $false)]
    [string] $IntegrationStorageAccount

)


<#
Note on Naming Variables
The Naming variables are used towards the end of the script. In the off chance users needed to change
the varable names I did not want users to look through the code. 
Names should match the names in the ARM templates. If you change the names here you need to change
the ARM templates.
#>

### Key Value Name
$KeyVaultName = $EnvironmentName + "kv"
    
### Azure Storage Staging Account
if([string]::IsNullOrEmpty($IntegrationStorageAccount))
{
    $storageAccountName = $EnvironmentName + "stg"
    
}
else {
    $storageAccountName = $IntegrationStorageAccount
}

### Logic App Name
$logicAppName = $EnvironmentName + "la"

### Azure Data Factory V2 Name
$adfName = $EnvironmentName + "adf"



Set-StrictMode -Version Latest

# Get current Az context
try {
    $azContext = Get-AzContext
} 
catch {
    throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
}

$resourceGroup = Get-AzResourceGroup -Name $EnvironmentName -ErrorAction SilentlyContinue

if (!$resourceGroup) {
    Write-Host "Creating Resource Group with name $EnvironmentName"

    New-AzResourceGroup -Name $EnvironmentName -Location $EnvironmentLocation | Out-Null
}

$keyVault = Get-AzKeyVault -VaultName $KeyVaultName 

if (!$keyVault) {
    Write-Host "Creating keyvault with the name $KeyVaultName"
        
    New-AzKeyVault -VaultName $KeyVaultName -ResourceGroupName $EnvironmentName -Location $EnvironmentLocation | Out-Null

    Write-Host "Keyvault created..."
}
else {
    Write-Host "Keyvault already created..."
    }

if ($azContext.Account.Type -eq "User") {
    Write-Host "Current context is user: $($azContext.Account.Id)"

    $currentUser = Get-AzADUser -UserPrincipalName $azContext.Account.Id

    #If this is guest account, we will try a search instead
    if (!$currentUser) {
        # External user accounts have UserPrincipalNames of the form:
        # myuser_outlook.com#EXT#@mytenant.onmicrosoft.com for a user with username myuser@outlook.com
        $tmpUserName = $azContext.Account.Id.Replace("@", "_")
        $currentUser = Get-AzureADUser -Filter "startswith(UserPrincipalName, '${tmpUserName}')"
        $currentObjectId = $currentUser.ObjectId
    } else {
        $currentObjectId = $currentUser.Id
    }

    if (!$currentObjectId) {
        throw "Failed to find objectId for signed in user"
    }
}
elseif ($azContext.Account.Type -eq "ServicePrincipal") {
    Write-Host "Current context is service principal: $($azContext.Account.Id)"
    $currentObjectId = (Get-AzADServicePrincipal -ServicePrincipalName $azContext.Account.Id).Id
}
else {
    Write-Host "Current context is account of type '$($azContext.Account.Type)' with id of '$($azContext.Account.Id)"
    throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command"
}

if ($currentObjectId) {
    Write-Host "Adding permission to keyvault for $currentObjectId"
    Set-AzKeyVaultAccessPolicy -VaultName $KeyVaultName -ObjectId $currentObjectId -PermissionsToSecrets Get, Set, List
}

try
{
    Write-Host "Starting ARM Deployment"
    New-AzResourceGroupDeployment -ResourceGroupName $EnvironmentName -TemplateFile "./Assets/arm_template_part1.json" -TemplateParameterFile "./Assets/arm_template.parameters.json" 

    ### Azure Batch ARM deployments do come online very fast. This break gives Azure Batch a chance
    ### to finish prior to moving on. 
    Write-Host "Taking quick break for ARM to finish deploying part 1"
    Start-Sleep -Seconds 60
    Write-Host "Starting part 2..."

    New-AzResourceGroupDeployment -ResourceGroupName $EnvironmentName -TemplateFile "./Assets/arm_template_part2.json" -TemplateParameterFile "./Assets/arm_template.parameters.json"

    ### Giving ARM a bit more time to finish"
    Write-Host "Taking quick break for ARM to finish deploying part 2"
    Start-Sleep -Seconds 60

    Write-Host "ARM deployment complete. Finishing up a few details."

    # Uploading the FHIR Tools for Anomization Code
    $storageAcctKey = (Get-AzStorageAccountKey -ResourceGroupName $EnvironmentName -AccountName $storageAccountName)| Where-Object {$_.KeyName -eq "key1"}
    $storageContext = New-AzStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAcctKey.Value

    Set-AzStorageBlobContent -Context $storageContext -Container "customactivity" -File "./Assets/AdfApplication.zip" -Blob "AdfApplication/AdfApplication.zip" -Force
    Set-AzStorageBlobContent -Context $storageContext -Container "customactivity" -File "./Assets/CustomActivity.ps1" -Blob "AdfApplication/CustomActivity.ps1" -Force 

    $logicAppMSI = (Get-AzResource -Name $logicAppName -ResourceType Microsoft.Logic/workflows).Identity.PrincipalId
    # Giving Logic App access to Key Vault
    Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $logicAppMSI -PermissionsToSecrets get,list -BypassObjectIdValidation

$LogicAppRoleExist = Get-AzRoleAssignment -ObjectId $logicAppMSI -ResourceGroupName $EnvironmentName -ResourceName $adfName -RoleDefinitionName "Data Factory Contributor" -ResourceType "Microsoft.DataFactory/factories"

if (!$LogicAppRoleExist) {
    
    # Giving Logic App access to Azure Data Factory V2
    New-AzRoleAssignment -ObjectId $logicAppMSI -ResourceGroupName $EnvironmentName -ResourceName $adfName -RoleDefinitionName "Data Factory Contributor" -ResourceType "Microsoft.DataFactory/factories"

}

# Giving Azure Data Factory V2 access to Key Vault
$adfmsi = (Get-AzDataFactoryV2 -ResourceGroupName $EnvironmentName -Name $adfName).Identity.PrincipalId
Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $adfmsi -PermissionsToSecrets get,list -BypassObjectIdValidation

Write-Host "Looks like we are all good here. Time to export data and do some research."
}

catch
{
    throw "Template deployment failed. With error(s) " + $_.Exception.Message
}
