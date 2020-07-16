# Secure FHIR Gateway and Proxy

Secure FHIR Gateway and Proxy is an Azure Function based solution that:
 + Acts as an intelligent and secure gateway to FHIR Servers
 + Allows multi-tenant access and purpose driven security policies specialized access to a common FHIR Server
 + Provides a consolidated approach to pre and post processing of FHIR Server Calls to support various access and result filtering or actions.</br>
 + Is Integrated with Azure Active Directory for authentication and to provide Role based access control.</br>
 + Acts as a FHIR specific reverse proxy rewriting responses and brokering requests to FHIR Servers</br>
## Authentication and RBAC Authorization
By default the proxy will configure and use Azure Active Directory (Azure AD) as an authentication provider.  You will also need to assign users/groups into specific server access roles in order to access the FHIR Server via the proxy.  You can also offload this responsibility to [API Management](https://azure.microsoft.com/en-us/services/api-management/)

## Pre and Post Processing Support
The proxy can be configured to execute any number of logic processing modules to support a variety of pre/post conditional actions on a per call basis. You can create custom processors by implementing the ```IProxyPreProcess``` or ```IProxyPostProcess``` interfaces in a thread safe class.
The modules are executed in a chained fashion determined by configured order.  Context is continually updated so the last result is passed to the next member of the processor chain resulting in a fully processed/filtered request or post-processing result.  Any configured module can stop the chain progression by issuing a do not continue command.

The base pre and post processing modules included and can be configured are:
 + ParticipantFilterPostProcess - This processing module will filter returned resources linked to a patient to only include patients where you are the patient or are a "Practitioner of Record" (e.g. in a participant role) Note: this only filters patient based linked resources. You can use this module as a basis for building your own security filtering</br>
 + PublishFHIREventPostProcess - This processing module will publish FHIR CUD events for resources to a configured eventhub.  These events can be subscribed too by any number of consumers in order to facilitate any number of orchestrated workflows. (e.g. CDS, Audits, Alerts, etc...)</br>
 + TransformBundlePreProcess - This processing module will transform incoming transaction bundle requests into batch bundle request and maintain UUID associations of contained resources.  This is a alternative for updating FHIR Servers unable to handle transaction based requests.</br>
 + DateSortPostProcessor - This processing module allows for date based sorting alternative on FHIR Servers that do not natively support _sort. The processor implements top level _sort=date or _sort=-date parameter for supported resources queries up to a configured maximum number of rows.</br>  
 + ProfileValidationPreProcess - This processing module adds the ability to call external profile (e.g. [US Core](https://www.hl7.org/fhir/us/core/)) and/or standard schema validation support for FHIR Servers who do not implement or support specific profile validation.

Check back often as more processing modules will be added. </br>
 
See [Proxy Configuration](##configuration) section below for full descriptions and configuration instructions.
## Reverse Proxy
All FHIR Server responses are re-written to include the proxy address as the appropriate endpoint so the FHIR Server URL is never directly exposed.

## Architecture Overview
![Fhirproxy Arch](fhirproxy_arch.png)


## Deploying your own FHIR Proxy

Please note you should deploy this proxy into a tenant that you control Application Registrations, Enterprise Applications, Permissions and Role Definitions Assignments

1. [Get or Obtain a valid Azure Subscription](https://azure.microsoft.com/en-us/free/)</br>
   _Note:Skip to Step 5 if you already have a FHIR Server/Service Client deployed_
2. [Deploy an Azure API for FHIR instance](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir-paas-portal-quickstart)
3. [Register a Service Client to Access the FHIR Server](https://docs.microsoft.com/en-us/azure/healthcare-apis/register-service-azure-ad-client-app).
4. [Find the Object Id for the Service Client and Register it with the FHIR Server](https://docs.microsoft.com/en-us/azure/healthcare-apis/find-identity-object-ids)
5. You will need the following information to configure the Secure FHIR Proxy 
   + Client/Application ID for the FHIR Service Client
   + The Client Secret for the FHIR Service Client
   + The AAD Tenant ID for the FHIR Server/Service Client
   + The Audience/Resource for the FHIR Server/Service Client typically https://<I>[yourfhirservername]</I>.azurehealthcareapis.com for Azure API for FHIR
6. [If you are running Windows 10 make sure you have enabled Windows Linux Subsystem](https://code.visualstudio.com/remote-tutorials/wsl/enable-wsl) and [Installed a Linux Distribution](https://code.visualstudio.com/remote-tutorials/wsl/install-linux)
7. [Install Azure CLI 2.0 on Linux based System or Windows Linux Subsystem](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-apt?view=azure-cli-latest)
8. [Download/Clone this repo](https://github.com/microsoft/health-architectures)
9. Open a bash shell into the Azure CLI 2.0 environment
10. Switch to the FHIR/FHIRproxy subdirectory of this repo ```cd FHIR/FHIRProxy```
11. Run the ```deployfhirproxy.bash``` script and follow the prompts
12. Congratulations you now have a Secure FHIR Proxy instance with authentication running. You can now add users/groups for authorized access (see below)

# Proxy Endpoint
The new endpoint for your FHIR Server should now be: ```https://<secure proxy url from above>/api/fhirproxy```. You can use any supported FHIR HTTP verb and any FHIR compliant request/query
For example to see conformance statement for the FHIR Server, use your browser and access the following endpoint:</br>
```https://<secure proxy url from above>/api/fhirproxy/metadata```

The endpoint will authenticate/authorize your access to the FHIR server will execute configured pre-processing routines, pass the modified request on to the FHIR Server via the configured service client, execute configured post-processing routines on the result and rewrite the server response to the client. 
The original user principal name and tenant are passed in custom headers to the FHIR server for accurate security and compliance auditing.  
_Note: You will need to login as a user/principal in a FHIR Reader and/or FHIR Administrative role to view. All proxy calls are auth secured including conformance_


## Adding Users/Groups for Access to the FHIR Server Proxy
At a minimum users must be placed in one or more FHIR server roles in order to access the FHIR Server via the Proxy. The Access roles are Administrator, Resource Reader and Resource Writer 
1. [Login to Azure Portal](https://portal.azure.com) _Note: If you have multiple tenants make sure you switch to the directory that contains the Secure FHIR Proxy_
2. [Access the Azure Active Directory Enterprise Application Blade](https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/StartboardApplicationsMenuBlade/AllApps/menuId/)
3. Change the Application Type Drop Down to All Applications and click the Apply button
4. Enter the application id or application name from above in the search box to locate the Secure FHIR Proxy application
5. Click on the Secure FHIR Proxy application name in the list
6. Click on Users and Groups from the left hand navigation menu
7. Click on the +Add User button
8. Click on the Select Role assignment box
9. Select the access role you want to assign to specific users
   The following are the predefined FHIR Access roles:
   + Administrator - Full Privileges to Read/Write/Link resource to the FHIR Server
   + Resource Reader - Allowed to Read Resources from the FHIR Server
   + Resource Writer - Allowed to Create, Update, Delete Resources on the FHIR Server
  
    When the role is selected click the select button at the bottom of the panel

10. Select the Users assignment box
11. Select and/or Search and Select registered users/guests that you want to assign the selected role too.
12. When all users desired have been selected click the select button at the bottom of the panel.
13. Click the Assign button.
14. Congratulations the select users have been assigned the access role and can now perform allowed operations against the FHIR Server

##  Configuration
The FHIR Proxy is configured on installation to be paired to a FHIR Server via a service client.  Default roles are added to the application and are configured for specific access in configuration settings section of the function app.
Enablement of pre/post processing modules is accomplished via the ```configmodules.bash``` utility.</br>

Important:  Most pre/post processing modules will require additional [configuration](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings) after enablement, in order to function. Please check the details of the module for instructions.

## Enabling Pre/Post Processing Modules
By default, no pre/post processors are configured to run.  You will need to enable and configure them following the steps below:

1. [Download/Clone this repo](https://github.com/microsoft/health-architectures) (if you have not done so)
2. Open a bash shell into the Azure CLI 2.0 environment
3. Switch to the FHIR/FHIRproxy subdirectory of this repo ```cd FHIR/FHIRProxy```
4. Run the ```configmodules.bash``` script and follow the prompts to launch the selector
5. Using DOWN/UP ARROW KEYS move to the Pre/Post Processing module(s) you want to enable/disable press the SPACE bar to toggle enabled(*)
6. To accept and configure selected processors TAB to OK and press ENTER, to abort TAB to Cancel and press ENTER
![ConfigureModulesScreen](configmods.png)


Note the utility does not read current configuration it will simply enable the modules you specify and update the function configuration. 
The utility requires the linux dialog utility [whiptail](https://howtoinstall.co/en/ubuntu/trusty/whiptail)

## Date Sort Post-Processor
This post process allows for date based sorting alternative on FHIR Servers that do not natively support _sort. The processor implements top level _sort=date or _sort=-date (reverse chron) query parameter for supported resource queries up to a hard maximum of 5000.</br>
The resources supported for top level_sort=date are: Observation,DiagnosticReport,Encounter,CarePlan,CareTeam,EpisodeOfCare and Claim. Any other resources will be ignored and not sorted.</br>
This processor is limited to process 5000 resource entries in a search-set bundle, it is imperative that you limit your query to not exceed this many resources for accurate results.
This processor also has the potential to cause server delays in responses for large results sets use with caution and limiting parameters.
A log warning will be logged for request that exceed the 5000 resource sort limit.</br>
This process requires no additional configuration.  

## Publish Event Post-Processor
This processor will publish FHIR Server Create/Update and Delete events for affected resources to a configured eventhub.  These events can be subscribed too by any number of consumers in order to facilitate any number of orchestrated workflows. (e.g. CDS, Audits, Alerts, etc...)</br>
In addition to the action date the eventhub message consists of the following information:
+ Action - HTTP Verb used to modify the FHIR Server
+ Resourcetype - The type of resource affected (e.g. Patient, Observation, etc...)
+ Id - The resource logical id on the FHIR Server that was affected.

You can use the data in the eventhub message to make decisions and get affected resource information to facilitate CDS or other workflows.

This process requires two configuration settings on the function app:
```
     EVENTHUB_CONNECTION: <A valid EventHub namespace connection string>
     EVENTHUB_NAME: <A valid event hub in the specified event hub namespace connection>
```

## Profile Validation Pre-Processor
This processor adds the ability to call external profile and/or standard schema validation support for FHIR Servers who do not implement or support specific profile validation.
This module expects external validation URLs to return an [OperationOutcome](https://www.hl7.org/fhir/operationoutcome.html) FHIR Resource.  The presence of issue entries will abort pre-processing and the FHIR Server call and the outcome will be returned to the client for resolution.

This process requires a configuration setting on the function app:
```
    FHIRVALIDATION_URL:<A valid URL to a compliant FHIR Validation Server>
```

The health-architectures [FHIR Validator](https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRValidator) provides a Docker wrapped version of the org.hl7 FHIR Validator and can be used with this processor.  It supports FHIR R4 and [US Core](https://www.hl7.org/fhir/us/core/) profiles.  To specify a profile(s) to validate against you can pass in valid US core profile references using the ```ms-fp-profile``` query parameter.
For example to validate a Patient resource for US Core compliance you would call the proxy with POST/PUT with the resource in the message body using the following url:

```
https://<secure proxy url from above>/api/fhirproxy/Patient?ms-fp-profile=http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient

```
These profile(s) will be extracted and passed to the FHIR Validator.

The validator also supports batch or transaction bundles as well analyzing each contained resource.  

## Transform Bundle Pre-Processor
This processing module will transform incoming transaction bundle requests into batch bundle request and maintain UUID associations of contained resources.  This is a alternative for updating FHIR Servers unable to handle transaction based requests.</br>
This processor will maintain internal logical id references when converted to batch mode, however, no transaction support will be included (e.g. Rollback for errors).  It will be the client responsibility to address any referential integrity or data issues that arise from server errors. Success or error status can be obtained using the batch-response bundle response.

This processor requires no additional configuration.


## Participant Filter Post-Processor
This module will filter returned resources linked to a patient to only include patients where you are the patient or are a "Practitioner of Record" (e.g. in a participant role) Note: this only filters patient based linked resources. You can use this module as a basis for building your own security filtering</br>


## How the Participant Post Processor works
![F H I R Proxy Seq](FHIRProxy_Seq.png)

## Configuring Participant Authorization Roles for Users
At a minimum users must be placed in one or more FHIR Participant roles in order to appropriately filter results from the FHIR Server. The Access roles are Patient, Practitioner and RelatedPerson. _Note:The user must also be in an appropriate Access role defined above_
1. [Login to Azure Portal](https://portal.azure.com) _Note: If you have multiple tenants make sure you switch to the directory that contains the Secure FHIR Proxy_
2. [Access the Azure Active Directory Enterprise Application Blade](https://ms.portal.azure.com/#blade/Microsoft_AAD_IAM/StartboardApplicationsMenuBlade/AllApps/menuId/)
3. Change the Application Type Drop Down to All Applications and click the Apply button
4. Enter the application id from above in the search box to locate the Secure FHIR Proxy application
5. Click on the Secure FHIR Proxy application in the list
6. Click on Users and Groups from the left hand navigation menu
7. Click on the +Add User button
8. Click on the Select Role assignment box
9. Select the access role you want to assign to specific users
   The following are the predefined FHIR Access roles:
   + Patient - This user is a patient and is linked to a Patient resource in the FHIR Server
   + Practitioner - This user is a practitioner and is linked to a Practitioner resource in the FHIR Server
   + RelatedPerson - This user is a relative/caregiver to a patient and is linked to a RelatedPerson resource in the FHIR Server
    
   When the role is selected click the select button at the bottom of the panel
10. Select the Users assignment box
11. Select and/or Search and Select registered users/guests that you want to assign the selected role too.
12. When all users desired have been selected click the select button at the bottom of the panel.
13. Click the Assign button.
14. Congratulations the select users have been assigned the participant role and can now be linked to FHIR Resources

## Linking Users in Participant Roles to FHIR Resources
1. Make sure you have configured Participant Authorization Roles for users
2. Obtain the FHIR Resource Id you wish to link to a AAD User principal.  Note you can use any search methods for the resources described in the FHIR specification.  It is strongly recommended to use a known Business Identifier in your query to ensure a specific and correct match.
   For example:
   To find a specific Patient in FHIR with a MRN of 1234567 you could issue the following URL in your browser:
   
   ```https://<your fhir proxy url>/api/fhirproxy/Patient?identifier=1234567```
   
   To find a specific Practitioner with last name Smith, in this case you can use other fields to validate like address, identifiers,etc... 
   ```https://<your fhir proxy address>/api/fhir/Practitioner?name=smith```
    
   The resource id is located in the id field of the returned resource or resource member in a search bundle
   ```"id": "3bdaac8f-5c8e-499d-b906-aab31633337d"``` 
 
   _Note: You will need to login as a user in a FHIR Reader and/or FHIR Administrative role to view._
 
 3. You will need to obtain the participant user principal name for the AAD instance in your tenant that are assigned and in roles for the secure proxy application.  Make sure the Role they are in corresponds to the FHIR Resource you are linking. 
    For example: ```somedoctor@sometenant.onmicrosoft.com```
 4. Now you can link the FHIR Resource to the user principal name by entering the following URL in your browser:
 
    ```https://<your fhir proxy url>/api/manage/link/<ResourceName>/<ResourceID>?name=<user principal name>``` 

    For example to connect Dr. Mickey in my AAD tenant principal who’s user name is mickey@myaad.onmicrosoft.com to the FHIR Practitioner Resource Id 3bdaac8f-5c8e-499d-b906-aab31633337d you would enter the following URL:
    ```https://<your fhir proxy url>/api/manage/link/Practitioner/3bdaac8f-5c8e-499d-b906-aab31633337d?name=mickey@myaad.onmicrosoft.com```
     
    _Note: You will need to login as a user in a FHIR Administrative role to perform the assignment_

5.  Your done the principal user is now connected in role to the FHIR resource.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

FHIR® is the registered trademark of HL7 and is used with the permission of HL7.
