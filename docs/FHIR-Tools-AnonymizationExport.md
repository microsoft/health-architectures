---
title: Export with Anonymization
parent: Tools
grand_parent: FHIR
nav_order: 1
---

# Export with Anonymization 

![Microsoft and FHIR](/assets/images/msft-fhir.png)

### Introduction
The FHIR Export with Anonymization is a template for creating an automated pipeline to process the bulk export for FHIR using Azure tools. The goal of the template is to enable quick and continuous creation of research datasets while applying HIPAA safe harbor rules.

The template connects multiple tools from the Microsoft Health Cloud and Data Team together into an automated solution. The two main components are:
1. Bulk Export or $export - https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data
2. FHIR Tools for Anonymization - https://github.com/microsoft/FHIR-Tools-for-Anonymization

### Architecture
The FHIR Export with Anonymization follows the architecture and steps below.

![FHIRExportAnonymizationArch](/assets/images/FHIRExportAnonymizationArch.jpg)

1. Timer triggers Logic App - 1:00 AM UTC is the default
2. Logic App calls FHIR service with GET $export
3. The FHIR service pushes bulk export to preset storage location
4. Logic App runs an Until loop waiting for $export operation to complete
5. Logic App sends the $export storage location information to Azure Data Factory
6. Azure Data Factory calls Azure Batch with the storage location information
7. Azure Batch performs the deidentification with the FHIR Tools for Anonymization
8. Azure Batch and Azure Data Factory put the deidentified data in a new output location (Azure Data Lake Gen 2)

The Big Data tools shown in the architecture are a representation of what you can use. They are not included in the template and therefore will not be deployed.

The Azure Logic App loops on a 5 minute time checking for the Bulk Export to finish. If the export is not complete the Logic App will wait 5 minutes then check again. The frequency is adjustable inside the Azure Logic App.

Note: The template does not go as far as applying networking rules. You may revise the template to include the rules for your environment, or manually adjust them after deployment.

### Prerequisites

Before deploying the pipeline, make sure you have:

- An Azure Subscription
- Azure API for FHIR deployed with data
- Powershell `Az` Module. Specifically `Az.Reources` 2.3.0 or higher.

If you need to install the latest version of the `Az` Powershell Module. Documentation for installing Azure PowerShell - <https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-4.2.0>

  ```PowerShell
  Install-Module Az
  ```

### Deployment

1. Clone this repo

    ```powershell
    git clone https://github.com/Microsoft/health-architectures
    ```

2. Navigate to health-architectures/FHIR/FHIRExportwithAnonymization and open the ./Assets/arm_template_parameters.json file in your preferred JSON editor. Replace FHIR URL, client id, client secret, tenant id and storage account with yours.

```json
{
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "fhirserver-url": {
            "value": "<<FHIR SERVER URL>>",
            "metadata": {
                "description":"https://<myfhir>.azurehealthcareapis.com  WARNING: make sure to remove the forward slash / after .com
                If you are using the FHIR Proxy enter the fhir proxy url."
            }
        },
        "fhirserver-clientid": {
            "value": "<<FHIR SERVER CLIENT ID>>"
        },
        "fhirserver-clientSecret": {
            "value": "<<FHIR SERVER CLIENT SECRET>>"
        },
        "fhirauth-tenantid": {
            "value": "",
            "metadata": {
                "description": "Supply only if FHIR authentication and the deployment subscription are not in the same tenant. If you are unsure leave "" or remove entire segment"
            }
        },
        "IntegrationStorageAccount":{
            "value": "",
            "metadata":{
                "description": "If the FHIR integration has already been setup with a storage account, then add the name of the storage account here. Otherwise a new storage account will be created."
            }
        }
    }
}
```

Save & close the parameters file.

Note - If you have not setup an Integration Storage Account. Do NOT setup one up. The script will create the storage account. You can link it to the FHIR service post deployment.

### Optional Setup

The FHIR Export with Anonymization uses the default settings in the Anonymization toolset. If you would like other settings please follow theses steps prior to deployment:

1. Find and copy the zip file ./Assets/AdfApplication.zip to a new temporary location
2. Unzip the file
3. In the unzipped folder locate the file called configuration-sample.json
4. Open the file and make your setting adjustments. Configuration file settings can be found here <https://github.com/microsoft/FHIR-Tools-for-Anonymization#configuration-file-format>
5. Save your configuration changes.
6. Zip the folder backup. Make sure the zipped folder is the same name 'AdfApplication.zip'.
7. Replace the zip file in the Assets folder with the new zip file.

### Deploy

### Log into Azure using PowerShell

1. Launch **PowerShell** on your machine. Keep PowerShell open until the end of this tutorial. If you close and reopen, you may need to run these commands again.

2. Make sure PowerShell is run in the correct directory

   ```powershell
   cd health-architectures\Research-and-Analytics\FHIRExportwithAnonymization
    ```

3. Connect to Azure: run the following command to log in. Follow the instructions for entering the Azure username and password.

    ```powershell
    Connect-AzAccount
    ```

4. Run the following command to view all the subscriptions for this account:

    ```powershell
    Get-AzSubscription
    ```

5. If you see multiple subscriptions associated with your account, run the following command to select the subscription that you want to work with. Replace **SubscriptionId** with the ID of your Azure subscription:

    ```powershell
    Select-AzSubscription -SubscriptionId "<SubscriptionId>"
    ```

6. Create the PowerShell variables required by the template and run the following command to deploy the pipeline:

   ```powershell
   $EnvironmentName = "<NAME HERE>" #The name must be lowercase, begin with a letter, end with a letter or digit, and not contain hyphens.
   $EnvironmentLocation = "<LOCATION HERE>" #optional input. The default is eastus2

   ./deployFHIRExportwithAnonymization.ps1 -EnvironmentName $EnvironmentName -EnvironmentLocation $EnvironmentLocation #Environment Location is optional
   ```

Deployment process may take 5 minutes or more to complete.

Common errors on deployment include:

- Azure Batch not enabled in the subscription
- Azure Batch already deployed the max number of times in a subscription

Warning: Azure Key Vault now defaults to soft-delete, a manual removal of the Azure Key Vault is required if you need to delete this deployment and start over.

### Post Deployment without pre-configured FHIR Integration Storage Account

If you did not setup the FHIR Integration Storage Account prior to deployment you should now. Locate the name of the storage account from the deployment. The default is the Environment Name with 'stg' appended to the end.

Then follow the instructions here for attaching the FHIR service to the storage account. <https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data>

IMPORTANT: Make sure you set the permissions between the FHIR service and the storage account. Step 2b in on the 'Configure export data' link.

### Post Deployment for pre-configured FHIR Integration Storage Account

If you did not setup the FHIR Integration Storage Account prior to deployment, there is one setting you need to change for the Azure Data Factory to work. We need to change the connection string for the blobstorageacctstring secret in the new key vault. If you are comfortable scripting  this feel free. However, here are the instructions using the portal.

Open the Azure Portal. Navigate to the new FHIR Integration Storage Account you listed in the ARM parameters file. Locate the storage account 'Access key' blade under 'Settings'. When the new blade opens copy one of the connection strings. We only need one of them either connection string will work.

With the connection string copied, close that blade. Navigate to the new key vault deployed with the script. The key vault should be in the new resource group and named the resource group with 'kv' appended to the end of the name. Open the key vault, look for the 'Secrets' blade under 'Settings'. When the blade opens, click on the secret named 'blobstorageacctstring'. Then click "+ New Version". In the 'Value' box paste the connection string from the storage account. Then click the 'Create' button at the bottom the page.

You are done. This will point the Azure Data Factory to the pre-configured FHIR Integration Storage Account.

### Optional Post Deployment

If you would like to adjust the start time or run interval, open the Logic App in the deployed resource group. The first step called 'Recurrence' is where the timer is stored.
