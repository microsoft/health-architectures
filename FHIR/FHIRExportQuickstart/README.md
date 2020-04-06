## API for FHIR Quickstart for $export


### Prerequisites
Before deploying make sure you have:
- An Azure Subscription
- Azure API for FHIR deployed with data
- Configured Export for API for FHIR. Documentation for config - https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data
- Installed the `Az` Powershell Module
  ```PowerShell

  Install-Module Az 
  ```


### Deployment
#### Setup parameters
1. Clone this repo

    ```powershell
    git clone https://github.com/Microsoft/health-architectures
    
    ```
2. navigate health-architectures/FHIRExportQuickStart and open the arm_template_parameters.json file in your perference json editor. Notepad works. 

```json
{
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "apiforfhir":{
            "value": "",
            "metadata": {
                "description":"https://<myfhir>.azurehealthcareapis.com  
                            WARNING: make sure to remove the forward slash / after .com"
            }
    },
        "tenantID":{
            "value": "",
            "metadata": {
                "description":"format should be eithers the tenant GUID or tenantname.onmicrosoft.com"
            }
    },
        "clientID":{
            "value": ""
    },
        "clientSecret":{
            "value": ""
            
    }
}
}
```
3. Fill out the parameter values with your inforamtion. Save & Close the file.

#### Log in to Azure using PowerShell
1. Launch **PowerShell** on your machine. Keep PowerShell open until the end of this tutorial. If you close and reopen, you may need to run these commands again.

2. Make sure PowerShell is in the correct directiory
   ```powershell
   cd health-architectures/FHIRExportQuickStart
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

./ deployFHIRExport.ps1 -Name $Name -Location $Location
```
7. When the PowerShell is finished you should have a LogicApp ready to run your export. You can manually trigger in the Azure Portal or but using this command.

```powershell
Start-AzLogicApp 
     -ResourceGroupName <Name of Logic App plus 'rg' added> 
     -Name <Name from above, you could put $Name here>
     -TriggerName "Recurrence"
```

