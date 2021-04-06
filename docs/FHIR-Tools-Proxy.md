---
title: Proxy
parent: Tools
grand_parent: FHIR
nav_order: 4
---

# Proxy Gateway

![Microsoft and FHIR](/assets/images/msft-fhir.png)

# FHIR Proxy Install Documentation

### Prerequisites

- Ensure you have access to an Azure Subscription
- Be sure to have previously deployed the Azure API for FHIR and have these handy:
  - FHIR Server URL
  - FHIR Server Service Client ID
  - FHIR Server Service Client Application ID
  - FHIR Server Service Client Secret
  - FHIR Server/Service Client Audience/Resource
  - Must have permissions to register Key Vault secrets
  - Registered a Service Client to access the FHIR Server
  - Locate the Object ID for the Service Client and Register it with the FHIR Server
  - Be sure you have permissions to register resources types
  - Be sure to have Contributor permission on the resource group you are deploying to
  - Have some familiarity with bash shell

### Install Instructions

- [Open Azure Cloud Shell](https://shell.azure.com) you can also access this from azure portal
- Select Bash Shell
- Clone this repo ```git clone https://github.com/microsoft/health-architectures```
- Change to the FHIR/FHIRproxy subdirectory of this repo cd FHIR/FHIRProxy ```cd ./health-architectures/FHIR/FHIRProxy/```
- Run the deployfhirproxy.bash script and follow the prompts -> ``` ./deployfhirproxy.bash```. If it successfully executes the script, your bash shell will look like this

![Portal View](/assets/images/deployfhirproxy.png)

- If you are using the Service Client with the proxy, you must be authenticated against an Azure AD Tenant where you can perform app registrations and grant API permissions. 
    - You may use a secondary tenant and create the service principal in that tenant (again you must be able to perform app registrations and grant API permissions in the secondary tenant). 
- You may use oauth code flow and it should work; service principals will need additional permissions

- Next you will be asked to choose an already existing resource group or to create a new resource group. (It’s probably best to create a new resource group) Should you choose to create a new resource group, the script will ask you to provide a name for the new resource group and enter the resource group location.

![Portal View](/assets/images/resourcegroup.png)

- Next enter your deployment prefix. Please be sure to enter a unique name as we do not want to the script to display errors. The prefix allows for a unique name to be created to avoid resource conflicts.

![Portal View](/assets/images/deploymentprefix.png)

- Next enter the proxy function app name. Feel free to create your own unique name

![Portal View](/assets/images/functionappname.png)

In the next few steps, you will need your FHIR Server details
- Enter the FHIR Server URL

![Portal View](/assets/images/FHIRURL.png)

- Then enter your FHIR Server Service Client ID. If the server is on the same tenant and you want to use MSI, you can leave this blank and it will set up the MSI object ID. You can find your tenant ID in the Azure Portal. 

![Portal View](/assets/images/FHIRServiceClient.png)

- Enter the FHIR Server Service Client Application ID

![Portal View](/assets/images/FHIRServiceApplication.png)

- Enter the FHIR server service Client Secret

![Portal View](/assets/images/FHIRServiceSecret.png)

- Enter the FHIR Server/Service Client Audience/Resource

![Portal View](/assets/images/FHIRServiceAudience.png)

Once these steps are complete the install will run through the automation and save the FHIR information in key vault. 

- The deployment will look something like this: 

![Portal View](/assets/images/DeploymentComplete.png)

And lastly the script text below will confirm the script ran successfully. 

![Portal View](/assets/images/ScriptSuccessful.png)


### After the Install


After your install Key Vault -> Settings -> Secrets will resemble the image below. 

*Note: FP is an abbreviation for FHIR Proxy and FS is an abbreviation for FHIR Server*

![Portal View](/assets/images/Keyvault.png)

**Postman Instructions**

Follow the guide below to create a new OAuth token correctly. 

![Portal View](/assets/images/postmansetup.png)

- Navigate to your FHIR Proxy collection and test a GET statement. When sending GET statement to the proxy you must use OAuth or your permission will be denied. This shows the proxy is responding. 

![Portal View](/assets/images/postmanmeta.png)

- Go to “Authorization” in postman and configure a New Token OAuth2.0. Using the steps below. 
- Create a Name for the new token
- Ensure the Grant Type is “Authorization Code” 

![Portal View](/assets/images/postmancallback.png)

- Add the Callback URL to the app registration of the FHIR Proxy

**Troubleshooting Steps**

- In Azure Active Directory under “App registrations” locate the proxy under “All Applications”. It will not be under “Owned Applications”.

![Portal View](/assets/images/appregistration.png)

- Click on your proxy and navigate to “Authentication” and add the call back URL from Postman under “Web”> Redirect URL’s. 

![Portal View](/assets/images/redirectURl.png)

- To locate the 3rd URL, go back to Postman, click on the “Authorize using browser” setting, and copy the link to Redirect URL’s. 

![Portal View](/assets/images/CallbackURL.png)

*Note: If you run into Authentication errors, it is usually OAuth and not the Proxy. Be sure to double check related fields in OAuth token for accuracy.*

- Under Enterprise Applications search for the proxy in “All Applications”

![Portal View](/assets/images/enterprise.png)

- Click on the fully qualified principal.

![Portal View](/assets/images/fullyqualified.png)

*Note: The Proxy creates 2 principals. The principal that is not fully qualified is managed by Azure and registered with Key vault. The fully qualified principal is the one we need for most trouble shooting scenarios.*

- Navigate to users and groups > Add User/Group and follow the prompts. You will add a user with Administrator permissions. Once completed your user will have the below permissions.

![Portal View](/assets/images/userpermission.png)

---

### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**
