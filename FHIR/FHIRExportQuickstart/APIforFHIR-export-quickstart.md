## API for FHIR Quickstart for $export


### Prerequisites
Before deploying make sure you have:
- Configured Export for API for FHIR. Documentation for config - https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data
- Installed the `Az` Powershell Module
  ```PowerShell

  Install-Module Az 
  ```

### Deployment
#### Log in to Azure using PowerShell
1. Launch **PowerShell** on your machine. Keep PowerShell open until the end of this tutorial. If you close and reopen, you need to run these commands again.

2. Run the following command, and enter the Azure user name and password to sign in to the Azure portal:

    ```powershell
    Connect-AzAccount
    ```

3. Run the following command to view all the subscriptions for this account:

    ```powershell
    Get-AzSubscription
    ```

4. If you see multiple subscriptions associated with your account, run the following command to select the subscription that you want to work with. Replace **SubscriptionId** with the ID of your Azure subscription:

    ```powershell
    Select-AzSubscription -SubscriptionId "<SubscriptionId>"
    ```

5. 