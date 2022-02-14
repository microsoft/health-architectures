# Postman Setup + sample Postman environments and collections 


## Overview 
[Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/overview) makes a REST API available for client applications to access and interact with FHIR data. When testing data connectivity between Azure API for FHIR and a client app, it is convenient to use an API testing utility to send requests, view responses, and debug issues. One of the most popular API testing tools is [Postman](https://www.postman.com/), and in this repo we provide a basic set of data files and instructions to help you get started using Postman to test Azure API for FHIR.

### Prerequisites
+ An **Azure API for FHIR endpoint**. To deploy Azure API for FHIR (a managed service), you can use the [Azure portal](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart), [PowerShell](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-powershell-quickstart), or [Azure CLI](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-cli-quickstart).
+ **FHIR-Proxy** deployed along with Azure API for FHIR. To learn more about FHIR-Proxy, please visit [here](https://github.com/microsoft/fhir-proxy).
+ **Postman** installed - desktop or web client. Please visit [here](https://www.getpostman.com/) for information about how to install Postman. 

### Getting started
To set up Postman for testing Azure API for FHIR, you will go through the following steps:

**Step 1:** Create an App Registration for Postman in AAD  
**Step 2:** Import environment and collection files into Postman  
**Step 3:** Configure two Postman environments: 
1. One Postman environment for making API calls directly to Azure API for FHIR  
2. Another Postman environment for making API calls to Azure API for FHIR through FHIR-Proxy  

**Step 4:** Get an authorization token from AAD  
**Step 5:** Practice making API calls to Azure API for FHIR  

### Step 1 - Create an App Registration for Postman in AAD 

You will need to create a registered [client application](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) for Postman to access Azure API for FHIR (this application registration will represent Postman in Azure Active Directory).

1. In Azure Portal, go to **Azure Active Directory** -> **App registrations** and make a **New registration**. 
2. Under **Redirect URI (optional)** select **Web** and then enter https://www.getpostman.com/oauth2/callback.
3. Click **Register**.
4. Then, click on your newly created App Registration and you will be taken to the app's **Overview** blade.
5. Click on **API Permissions**.
6. Click on **Add a permission**.
7. Select the **My APIs** tab.
8. Click on the FHIR-Proxy instance that you deployed.
9. Click on **Delegated permissions**.
10. Scroll down and select **user_impersonation**.
11. Click **Add permissions**.
12. When back in the **API permissions** blade, click **Add a permission** (again).
13. Select **APIs my organization uses** tab.
14. Type in "Azure Healthcare APIs" and select the item in the list.
15. Scroll down and select **user_impersonation**.
16. Click **Add permissions**.
17. When back in the **API permissions** base blade, click **Certificates and secrets**.
18. Click **New client secret**.
19. Enter a name for the secret in the **Description** field.
20. Click **Add**.
21. Copy the secret **Value** and securely store it somewhere.

For more information on registering client applications in AAD, please review the [Service Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-service-azure-ad-client-app) and [Confidential Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) documentation for Azure API for FHIR. 

Be sure to register https://www.getpostman.com/oauth2/callback as the reply URL in your client application registration! (see above).

__Note:__ In order to access Azure API for FHIR directly (i.e., bypassing FHIR-Proxy), make sure you have assigned the "FHIR Data Contributor" role in the Postman application registration **App roles** blade. Also, make sure that you have assigned the "FHIR Data Contributor" role to your own user account. For more information, see [Configure Azure RBAC for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/configure-azure-rbac).

### Step 2 - Import environment and collection files into Postman

1. Copy the JSON formatted Postman environment template for Azure API for FHIR from below and paste into a text editor of your choice.

```
{
	"id": "225b239b-1eb3-46fe-bf41-77e4c7ea339d",
	"name": "api-fhir",
	"values": [
		{
			"key": "tenantId",
			"value": "",
			"enabled": true
		},
		{
			"key": "clientId",
			"value": "",
			"enabled": true
		},
		{
			"key": "clientSecret",
			"value": "",
			"enabled": true
		},
		{
			"key": "bearerToken",
			"value": "",
			"enabled": true
		},
		{
			"key": "resource",
			"value": "",
			"enabled": true
		},
		{
			"key": "fhirurl",
			"value": "",
			"enabled": true
		}
	],
	"_postman_variable_scope": "environment",
	"_postman_exported_at": "2021-08-12T02:06:11.383Z",
	"_postman_exported_using": "Postman/8.10.0"
}
```

2. Save the file in your local desktop environment (accessible from Postman) as "api-for-fhir.postman_environment.json". This file is also accessible in this repo [here](./api-for-fhir/api-for-fhir.postman_environment.json).

3. Copy the JSON formatted Postman environment template for FHIR-Proxy from below and paste into a text editor of your choice.

```
{
	"id": "225b239b-1eb3-46fe-bf41-77e4c7ea339d",
	"name": "fhir-proxy",
	"values": [
		{
			"key": "tenantId",
			"value": "",
			"enabled": true
		},
		{
			"key": "clientId",
			"value": "",
			"enabled": true
		},
		{
			"key": "clientSecret",
			"value": "",
			"enabled": true
		},
		{
			"key": "bearerToken",
			"value": "",
			"enabled": true
		},
		{
			"key": "resource",
			"value": "",
			"enabled": true
		},
		{
			"key": "fhirurl",
			"value": "",
			"enabled": true
		}
	],
	"_postman_variable_scope": "environment",
	"_postman_exported_at": "2021-08-12T02:06:11.383Z",
	"_postman_exported_using": "Postman/8.10.0"
}
```
4. Save the file in your local desktop environment (accessible from Postman) as "fhir-proxy.postman_environment.json". This file is also accessible in this repo [here](./fhir-proxy/fhir-proxy.postman_environment.json).

5. Create a new Postman Workspace (or select an existing one if already created).

6. Click the ```Import``` button next to your workspace name. 

7. Import the ```api-for-fhir.postman_environment.json``` file that you saved to your desktop environment.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

8. Import the ```fhir-proxy.postman_environment.json``` file that you saved to your desktop environment.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

8. Access the ```FHIR-CALLS.postman-collection.json``` file available in this repo [here](./api-for-fhir/FHIR-CALLS.postman_collection.json) and save the file to your desktop environment. Then import the file into Postman.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

9. 6. Access the ```FHIR_Search.postman_collection.json``` file available in this repo [here](./api-for-fhir/FHIR_Search.postman_collection.json) and save the file to your desktop environment. Then import the file into Postman.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

Now you will need to configure your two Postman environments (`api-for-fhir` and `fhir-proxy`) by retrieving values for `tenantId`, `clientId`, `clientSecret`, `fhirurl`, and `resource` from Azure Portal for Azure API for FHIR and FHIR-Proxy, respectively. Then you will populate the two environments (`api-for-fhir` and `fhir-proxy`) in Postman.
 
### Step 3 - Configure Postman environments
Before you can access Azure API for FHIR (hereto "FHIR service"), you'll need to create or update the following Postman environment variables.

- ```fhirurl``` – The FHIR service full URL. For example, https://xxx.azurehealthcareapis.com. It's located in the FHIR service overview menu option.
- ```bearerToken``` – The variable to store the Azure Active Directory (Azure AD) access token in the script. Leave it blank.
- FHIR server URL (for example, https://MYACCOUNT.azurehealthcareapis.com)
- Identity provider Authority for your FHIR server (for example, https://login.microsoftonline.com/{TENANT-ID})
- Audience, which is usually the URL of the FHIR server (for example, https://<FHIR-SERVER-NAME>.azurehealthcareapis.com or https://azurehealthcareapis.com).
- ```client_id``` or application ID of the confidential client application used for accessing the FHIR server.
- ```client_secret``` or application secret of the confidential client application.

Postman Env variable | Azure Setting          | Variable Type 
---------------------|------------------------|--------------
tenantId             | Azure AD Tenant ID     | GUID 
clientId             | Azure AD Client ID     | GUID
clientSecret         | Azure AD Client Secret | Secret 
bearerToken          | Auto-Populated         | Token
fhirurl              | FHIR Endpoint          | URL
resource             | FHIR Endpoint          | URL

#### Postman environment setup for Azure API for FHIR access via Microsoft FHIR-Proxy 
If you haven't already imported the Postman environment file for FHIR-Proxy, the ```./fhir-proxy``` directory in this repo contains a sample Postman Environment to help users get started. 

__NOTE__ All FHIR-Proxy Postman configuration parameter values can be found in the Key Vault installed during setup. 

Postman Env variable | Azure Setting          | Variable Type 
---------------------|------------------------|--------------
tenantId             | Azure AD Tenant ID     | GUID 
clientId             | Azure AD Client ID     | GUID
clientSecret         | Azure AD Client Secret | Secret 
bearerToken          | Auto-Populated         | Token
fhirurl              | FHIR Endpoint          | URL
resource             | FHIR Endpoint          | URL

### Step 4 - Get an authorization token from AAD
Azure API for FHIR is secured by Azure AD, and this cannot be disabled as this is the default platform for authentication. To access Azure API for FHIR, you must get an Azure AD access token first. For more information, see [Microsoft identity platform access tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens).

To obtain an access token via Postman, you will need to send a ```POST AuthorizeGetToken``` request. For the ```POST AuthorizeGetToken``` request to succeed, the call must be set up as follows with the ```{{}}``` parameter values stored in the Postman environment:

URL: https://login.microsoftonline.com/{{tenantid}}/oauth2/token

Body tab set to x-www-form-urlencoded and key value pairs:
- grant_type: Client_Credentials
- client_id: {{clientid}}
- client_secret: {{clientsecret}}
- resource: {{fhirurl}}

Test to set the ```bearerToken``` variable
```json
var jsonData = JSON.parse(responseBody);
postman.setEnvironmentVariable("bearerToken", jsonData.access_token);
```
On clicking ```Send``` you should see a response with the Azure AD access token, which is saved to the variable ```accessToken``` automatically. You can then use it in subsequent API calls made to Azure API for FHIR. 

__Note:__ Access tokens expire after 60 minutes. To obtain a token refresh, simply make another ```POST AuthorizeGetToken``` call and the token will be valid for another 60 minutes.

### Testing Setup 
Testing FHIR-Proxy/Azure API for FHIR with Postman begins simply by adding values for the Postman environment variables in your two Postman environments (`api-for-fhir` and `fhir-proxy`).

1) If you haven't already, download the sample environment templates for both [Azure API for FHIR](./api-for-fhir/api-for-fhir.postman_environment.json) and [FHIR-Proxy](./fhir-proxy/fhir-proxy.postman_environment.json).   

2) Open Postman (web or client) and import the Postman environment templates and collections files (`FHIR-CALLS.postman_collection.json` and `FHIR_Search.postman_collection.json`).

3) Go to Environments in Postman, select the `api-for-fhir` Environment and enter in the variable information from Azure Portal (skip the `bearerToken` for now).

4) Go to Environments in Postman, select the `fhir-proxy` Environment and enter in the variable information from Azure Portal (skip the `bearerToken` for now).

The completed Environment should look something like this:

Environment variables ![Environment_variables](./docs/images/environment_variables_example.png)

Remember to set your "active" environment before going to the next step ![Environment_variables](./docs/images/environment_selection.png).

4) Go to Collections, select the `FHIR Calls` collection to open it then select List Metadata. It should look like this example: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls01.png)

5) Click `Send` (see above) to test the FHIR URL setup and the basic functions of your Azure API for FHIR. This command does not use Auth (by design) and it returns your FHIR Service Capability Statement. 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_metadata.png)

6) Next select `POST AuthorizeGetToken`. Note there are values in the call tabs: Authorization, Headers, Body, and Tests. This will call the Azure AD Tenant with your ClientID, ClientSecret, and Resource in the Body to obtain a Token.  On receipt of the Token, it is parsed into the bearerToken value. The result should look like this: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_token.png)

The rest of the calls use the token from the step above to Authenticate requests to the FHIR Service.  

### Resources 

A tutorial for using Postman with Azure API for FHIR is available on [docs.microsoft.com](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/access-fhir-postman-tutorial).
 
### FAQ's / Issues 

403 - Unauthorized:  Check the Azure RBAC for FHIR service [link](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir/configure-azure-rbac-for-fhir)

  
