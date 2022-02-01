# Sample Postman Environments and Collections 


## Overview 
Azure API for FHIR makes a REST API available for client applications to access and interact with FHIR data. When testing data connectivity between Azure API for FHIR and a client app, it is convenient to use an API testing utility to send requests, view responses, and debug issues. One of the most popular API testing tools is [Postman](https://www.postman.com/), and in this repo we provide a basic set of data files and instructions to help you get started testing Azure API for FHIR with Postman.

A tutorial for using Postman with Azure API for FHIR is available on [docs.microsoft.com](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/access-fhir-postman-tutorial).

The Postman [Environments](https://learning.postman.com/docs/sending-requests/managing-environments/) and [Collections](https://learning.postman.com/docs/getting-started/creating-the-first-collection/#:~:text=Postman%20Collections%20are%20a%20group,particular%20request%20in%20your%20history.) offered in this repo have been tested with Postman v8 (online and desktop client).


### Prerequisites
+ An Azure API for FHIR endpoint. To deploy Azure API for FHIR (a managed service), you can use the [Azure portal](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart), [PowerShell](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-powershell-quickstart), or [Azure CLI](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-cli-quickstart).

+ A registered [client application](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) to access Azure API for FHIR (this application registration will represent Postman in Azure Active Directory).

__NOTES__ 
- Recommendation is to register a [Service Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-service-azure-ad-client-app) or [Confidential Client](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/register-confidential-azure-ad-client-app) application (to represent Postman in AAD).
- Be sure to register a reply URL of https://www.getpostman.com/oauth2/callback in your client application registration.

In order to access Azure API for FHIR, make sure you have assigned the "FHIR Data Contributor" roll to the client application and to your own user account. For more information, see [Configure Azure RBAC for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/configure-azure-rbac).


## Auth - AAD and Tokens 
Azure API for FHIR is secured by Azure AD. The default authentication cannot be disabled. To access Azure API for FHIR, you must get an Azure AD access token first. For more information, see [Microsoft identity platform access tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens).

The POST request AuthorizeGetToken has the following:

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
On clicking Send you should see a response with the Azure AD access token, which is saved to the variable ```accessToken``` automatically. You can then use it in all FHIR service API requests.

See [FHIR-CALLS](./docs/fhirCalls.md) for additional information about this Postman collection. 

 
## Azure API for FHIR access
To access Azure API for FHIR, we'll need to create or update the following variables.

- ```fhirurl``` – The FHIR service full URL. For example, https://xxx.azurehealthcareapis.com. It's located from the FHIR service overview menu option.
- ```bearerToken``` – The variable to store the Azure Active Directory (Azure AD) access token in the script. Leave it blank.
- FHIR server URL (for example, https://MYACCOUNT.azurehealthcareapis.com)
- Identity provider Authority for your FHIR server (for example, https://login.microsoftonline.com/{TENANT-ID})
- Audience, which is usually the URL of the FHIR server (for example, https://<FHIR-SERVER-NAME>.azurehealthcareapis.com or https://azurehealthcareapis.com).
- client_id or application ID of the confidential client application used for accessing the FHIR service.
- client_secret or application secret of the confidential client application.

Postman Env variable | Azure Setting          | Variable Type 
---------------------|------------------------|--------------
tenantId             | Azure AD Tenant ID     | GUID 
clientId             | Azure AD Client ID     | GUID
clientSecret         | Azure AD Client Secret | Secret 
bearerToken          | Auto-Populated         | Token
fhirurl              | FHIR Endpoint          | URL
resource             | FHIR Endpoint          | URL


## API for FHIR access via Microsoft FHIR-Proxy 
Creating Postman collections for [Microsoft's FHIR-Proxy](https://github.com/microsoft/fhir-proxy) and its various components can be daunting when starting from scratch. The ./fhir-proxy directory in this repo contains a sample Postman Environment to help users get started. 

__NOTE__ All FHIR-Proxy Postman configurations can be found in the Key Vault installed during setup. 

Postman Env variable | Azure Setting          | Variable Type 
---------------------|------------------------|--------------
tenantId             | Azure AD Tenant ID     | GUID 
clientId             | Azure AD Client ID     | GUID
clientSecret         | Azure AD Client Secret | Secret 
bearerToken          | Auto-Populated         | Token
fhirurl              | FHIR Endpoint          | URL
resource             | FHIR Endpoint          | URL


## Testing Setup 
Testing FHIR-Proxy/Azure API for FHIR with Postman begins simply by adding values to the Environment variables.

1) Download the sampes environments in either [API-FHIR](https://github.com/daemel/fhir-postman/tree/main/api-for-fhir) or [FHIR-Proxy](https://github.com/daemel/fhir-postman/tree/main/fhir-proxy).   

2) Open Postman (web or client), import the Environment (ie api-for-fhir-postman_environment.json) and the Collections (FHIR-CALLS.postman_collection.json).

3) Go to Environments in Postman, select the Environment (api-fhir or fhir-proxy) and enter in the variable information (skip the bearer token) from your Client Application and the FHIR Service.

The completed Environment should look something like this

Environment variables ![Environment_variables](./docs/images/environment_variables_example.png)

Remember to set your "active" environment before going to the next step ![Environment_variables](./docs/images/environment_selection.png).

4) Go to Collections, select the FHIR Calls collection to open it then select List Metadata, it should look like this example: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls01.png)

5) Click Send (see above) to test the FHIR URL setup and the basic functions of your Azure API for FHIR. This command does not use Auth (by design) and it returns your FHIR Service Capability Statement. 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_metadata.png)

6) Next Select AuthorizeGetToken. Note there are values in the call tabs: Authorization, Headers, Body, and Tests. In short this will call the Azure AD Tenant with your ClientID, ClientSecret, and Resource in the Body to obtain a Token.  On receipt of the Token, it is parsed into the bearerToken value. The result should look like this: 

FHIR Calls ![FHIR_Calls](./docs/images/fhir-calls_token.png)

The rest of the calls use the token from the step above to Authenticate requests to the FHIR Service.  

## Resources 

[Access the FHIR service using Postman tutorial](https://docs.microsoft.com/en-us/azure/healthcare-apis/use-postman)

 
## FAQ's / Issues 

403 - Unauthorized:  Check the Azure RBAC for FHIR service [link](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir/configure-azure-rbac-for-fhir)

  
