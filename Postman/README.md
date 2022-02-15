# Postman Setup + sample Postman environments and collections 

## Overview 
When testing data connectivity between [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/overview) and a client app, it is useful to have an API testing utility to send requests, view responses, and debug issues. One of the most popular API testing tools is [Postman](https://www.postman.com/), and in this guide we provide instructions and a basic set of data files to help you get started using Postman to test Azure API for FHIR.

## Prerequisites
+ An **Azure API for FHIR endpoint**. To deploy Azure API for FHIR (PaaS), you can use the [Azure portal](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart), [PowerShell](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-powershell-quickstart), or [Azure CLI](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-cli-quickstart).
+ **FHIR-Proxy** deployed along with Azure API for FHIR. To learn more about FHIR-Proxy (OSS), please visit [here](https://github.com/microsoft/fhir-proxy).
+ **Postman** installed - desktop or web client. See [here](https://www.getpostman.com/) for information about how to install Postman. 

## Getting started
To set up Postman for testing Azure API for FHIR, we'll walk through these steps:

**Step 1:** Create an App Registration for Postman in AAD  
**Step 2:** Assign app roles to Postman for Azure API for FHIR and FHIR-Proxy  
**Step 3:** Import environment templates and collection files into Postman  
**Step 4:** Enter parameter values for two Postman environments: 
1. One environment for making API calls directly to Azure API for FHIR  
2. Another environment for making API calls via FHIR-Proxy to Azure API for FHIR 

**Step 5:** Get an authorization token from AAD  
**Step 6:** Test Postman setup and practice making API calls to Azure API for FHIR

## Step 1 - Create an App Registration for Postman in AAD 

Before you can use Postman to make API calls to Azure API for FHIR, you will need to create a registered [client application](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) to represent Postman in Azure Active Directory.

1. In Azure Portal, go to **Azure Active Directory** -> **App registrations** and make a **New registration**.
2. Type in a name for your application registration in the **Name** field. 
3. Scroll down, and under **Redirect URI (optional)** select **Web** and then enter https://www.getpostman.com/oauth2/callback.
4. Click **Register**.
5. Then, click on your newly created App Registration and you will be taken to the app's **Overview** blade.
6. Click on **API Permissions**.
7. Click on **Add a permission**.
8. Select the **My APIs** tab.
9. Click on the FHIR-Proxy instance that you deployed.
10. Click on **Delegated permissions**.
11. Scroll down and select **user_impersonation**.
12. Click **Add permissions**.
13. Make sure to click **Grant admin consent** (blue checkmark).
14. When back in the **API permissions** blade, click on **Add a permission** (again). 
15. Repeat #8 and 9.
16. Click on the **Application permissions** box on the right.
17. Select **Resource Reader** and click **Add permissions**.
18. Make sure to click **Grant admin consent** (blue checkmark).
19. When back in the **API permissions** blade for your Postman app registration, click **Add a permission** (again).
20. Select **APIs my organization uses** tab.
21. Type in "Azure Healthcare APIs" and select the item in the list.
22. Scroll down and select **user_impersonation**.
23. Click **Add permissions**.
24. Make sure to click **Grant admin consent** (blue checkmark).
25. When back in the **API permissions** blade, click **Certificates and secrets**.
26. Click **New client secret**.
27. Enter a name for the secret in the **Description** field.
28. Click **Add**.
29. Copy the secret **Value** and securely store it somewhere (you will need this later when you configure your Postman environment).

For more information on registering client applications in AAD, please review the [Service Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-service-azure-ad-client-app) and [Confidential Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) documentation for Azure API for FHIR. 

## Step 2 - Assign user/app roles to Postman for Azure API for FHIR and FHIR-Proxy  

1. In Azure Portal, go to **Home** -> **Resource groups** and find the resource group containing your Azure API for FHIR instance.
2. Click on your Azure API for FHIR instance in the list.
3. Go to the **Access Control (IAM)** blade.
4. Click on the **Roles** tab.
5. Click on **+ Add** -> **Add role assignment**.
6. Under the **Role** tab, select **FHIR Data Contributor** and then click **Next**.
7. Under the **Members** tab, select **User, group, or service principal**.
8. Click on **+ Select members**.
9. Type in the name of your Postman app registration in the **Select** field, highlight it, and click **Select**.
10. Under the **Review + assign** tab, click **Review + assign**.
11. When back in the **Access Control (IAM)** blade, under the **Roles** tab select **FHIR Data Contributor** (again) and then click **+ Add** -> **Add role assignment** (again).
12. Select **FHIR Data Contributor** under the **Role** tab and click **Next**.
13. Under the **Members** tab, select **User, group, or service principal**.
14. Click on **+ Select members**, type in your name or username, highlight it, and click **Select**.
15. Under the **Review + assign** tab, click **Review + assign**.
16. Now go to **Azure Active Directory** -> **Enterprise applications**.
17. Search for your FHIR-Proxy function app and select it from the list. It might be easiest to search by **Created on** date.
18. You will be taken to the **Overview** blade for your FHIR-Proxy Enterprise Application.
19. Go to the **Users and groups** blade.
20. Under **Add Assignment**, click on **None Selected** under **Users**.
21. Under **Users**, type in your name or username in the search field, click on it, and then click **Select**.
22. Click on **None Selected** under **Select a role**.
23. Under **Select a role**, click on **Resource Writer** and then click **Select**.
24. Under **Add Assignment**, click on **Assign**.

For more information on assigning user/app roles, see [Configure Azure RBAC for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/configure-azure-rbac).

## Step 3 - Import environment and collection files into Postman

1. Access the Postman environment template for Azure API for FHIR [here](./api-for-fhir/api-for-fhir.postman_environment.json). Save the file in your local desktop environment for later use.

2. Access the Postman environment template for FHIR-Proxy [here](./fhir-proxy/fhir-proxy.postman_environment.json). Save the file in your local desktop environment for later use.

3. Create a new Postman Workspace (or select an existing one if already created).

4. Click the ```Import``` button next to your workspace name. 

5. Import the ```api-for-fhir.postman_environment.json``` file that you saved to your desktop environment.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

6. Import the ```fhir-proxy.postman_environment.json``` file that you saved to your desktop environment.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

7. Access the ```FHIR-CALLS.postman-collection.json``` file available in this repo [here](./api-for-fhir/FHIR-CALLS.postman_collection.json) and save the file to your desktop environment. Then import the file into Postman.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

8. Access the ```FHIR_Search.postman_collection.json``` file available in this repo [here](./api-for-fhir/FHIR_Search.postman_collection.json) and save the file to your desktop environment. Then import the file into Postman.
    + Add the file to Postman using the ```Upload Files``` button or paste in the contents of the file using the ```Raw text``` tab.

Now you will need to configure your two Postman environments (`api-for-fhir` and `fhir-proxy`) by retrieving values for `tenantId`, `clientId`, `clientSecret`, `fhirurl`, and `resource` from Azure Portal for Azure API for FHIR and FHIR-Proxy, respectively. Then you will populate the two environments (`api-for-fhir` and `fhir-proxy`) in Postman.
 
## Step 4 - Configure Postman environments
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

### Postman environment setup for Azure API for FHIR access via Microsoft FHIR-Proxy 
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

## Step 5 - Get an authorization token from AAD
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

## Step 6 - Test setup and practice making API calls to Azure API for FHIR 
Testing FHIR-Proxy/Azure API for FHIR with Postman begins simply by adding values for the Postman environment variables in your two Postman environments (`api-for-fhir` and `fhir-proxy`).

1) If you haven't already, download the sample environment templates for both [Azure API for FHIR](./api-for-fhir/api-for-fhir.postman_environment.json) and [FHIR-Proxy](./fhir-proxy/fhir-proxy.postman_environment.json).   

2) Open Postman (web or client) and import the Postman environment templates and collections files (`FHIR-CALLS.postman_collection.json` and `FHIR_Search.postman_collection.json`).

3) Go to Environments in Postman, select the `api-for-fhir` Environment, and enter in the variable information from Azure Portal (skip the `bearerToken` for now).

4) Go to Environments in Postman, select the `fhir-proxy` Environment, and enter in the variable information from Azure Portal (skip the `bearerToken` for now).

The completed `api-for-fhir` Postman environment should look something like this:

Environment variables ![Environment_variables](./docs/images/environment_variables_example.png)

Remember to set your "active" environment before going to the next step ![Environment_variables](./docs/images/environment_selection.png).

5) Go to Collections, select the `FHIR Calls` collection to open it then select List Metadata. It should look like this example: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls01.png)

6) Click `Send` (see above) to test the FHIR URL setup and the basic functions of your Azure API for FHIR. This command does not use Auth (by design) and it returns your FHIR Service Capability Statement. 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_metadata.png)

7) Next select `POST AuthorizeGetToken`. Note there are values in the call tabs: Authorization, Headers, Body, and Tests. This will call the Azure AD Tenant with your ClientID, ClientSecret, and Resource in the Body to obtain a Token.  On receipt of the Token, it is parsed into the bearerToken value. The result should look like this: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_token.png)

The rest of the calls use the token from the step above to Authenticate requests to the FHIR Service.  

### Resources 

A tutorial for using Postman with Azure API for FHIR is available on [docs.microsoft.com](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/access-fhir-postman-tutorial).
 
### FAQ's / Issues 

403 - Unauthorized:  Check the Azure RBAC for FHIR service [link](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir/configure-azure-rbac-for-fhir)

  
