---
title: Export Quickstart
parent: Tools
grand_parent: FHIR
nav_order: 1
---

# Export Quickstart 

![Microsoft and FHIR](/assets/images/msft-fhir.png)

### Introduction 
The FHIR Export Quickstart is to help setup exporting FHIR data on a regular basis. Below is a simple architecture and steps.

![FHIRExportQuickstartArchitecture](/assets/images/FHIRExportQuickstartArchitecture.jpg)

Steps in data flow:
1. Timer triggers Logic App - default trigger time is 1:00 AM UTC
2. Logic Apps calls Key Vault for secrets
3. Logic App – calls FHIR service with GET $export
4. FHIR service pushed bulk export to preset storage location

### Prerequisites
Before deploying make sure you have:
- An Azure Subscription
- Azure API for FHIR deployed with data
- Configured Export for API for FHIR. Documentation for config - https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data
- Installed the Az Powershell Module

```PowerShell
Install-Module Az
```

### Deployment

1. Clone this repo

    ```powershell
    git clone https://github.com/Microsoft/health-architectures
    ```

2. Navigate to health-architectures/FHIRExportQuickStart and open the arm_template_parameters.json file in your preferred JSON editor. Notepad works.

    ```json
    {
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "fhirserver-url": {
            "value": "",
            "metadata": {
                "description":"https://<myfhir>.azurehealthcareapis.com  WARNING: make sure to remove the forward slash / after .com If you are using the FHIR Proxy enter the fhir proxy url."
            }
        },
        "fhirserver-clientid": {
            "value": ""
        },
        "fhirserver-clientSecret": {
            "value": ""
        },
        "fhirauth-tenantid": {
            "value": "",
            "metadata": {
                "description": "Supply only if fhir authentication and the deployment subscription are not in the same tenant. If you are unsure leave NULL or remove segment"
            }
        }
    }
    }
    ```

3. Fill out the parameter values with your information. Save & Close the file.

### Log in to Azure using PowerShell

1. Launch **PowerShell** on your machine. Keep PowerShell open until the end of this tutorial. If you close and reopen, you may need to run these commands again.

2. Make sure PowerShell is in the correct directory

   ```powershell
   cd health-architectures\Research-and-Analytics\FHIRExportQuickStart
    ```

3. Connect to Azure: run the following command. Follow the instructions for entering the Azure username and password.

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

6. Create the PowerShell variables and run the following command:

    ```powershell
    $Name = "<NAME HERE>", #default is fhirexport
    $Location = "<LOCATION HERE>" #default is eastus

    ./deployFHIRExport.ps1 -EnvironmentName $Name -EnvironmentLocation $Location
    ```

7. When the PowerShell is finished you should have a LogicApp ready to run your export. You can manually trigger in the Azure Portal or by using this command.

```powershell
Start-AzLogicApp
     -ResourceGroupName <Name of Logic App plus 'rg' added>
     -Name <Name from above, you could put $Name here>
     -TriggerName "Recurrence"
```

### Change Log

June 24, 2020

1. Improved README documentation
2. Added Key Vault support for sensitive information
3. Turned on secure output on the API calls to key vault. This will prevent FHIR client id and secrets from being written to logs
4. Cleaned up the parameter names to match the naming of the FHIR Export with Anonymization

### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**