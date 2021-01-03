---
title: Power Platform Connectors
parent: Tools
grand_parent: FHIR
nav_order: 1
---

# Power Platform App and Automation Connector Setup 

![Microsoft and FHIR](/assets/images/msft-dynamics-fhir.png)

Power Platform App and Automation Connector Setup

FHIRBase and FHIRClinical are connectors that allow for building health applications that help enable interoperability and ease of use of FHIR.

### Prerequisites
- Power Platform Access
- Azure API for FHIR or OSS FHIR Server deployed with data

### Deployment
There are several forms of deployment available, depending on your situation and / or need.

### Power Platform Deployment
Power Admin - Centeralized deployment instructions can be found here https://docs.microsoft.com/en-us/power-platform/admin/onpremises-data-gateway-source-management

Portal Deployment - Once a Data Connector is provisioned it can be consumed from the Power App or Power Automate portal, instructions and examples can be found here https://docs.microsoft.com/en-us/powerapps/maker/canvas-apps/connections-list

### GitHub Deployment
Power Platform Connectors are available on GitHub https://github.com/microsoft/PowerPlatformConnectors. The certified-connectors folder contains certified connectors which are already deployed and available out-of-box within the Power Platform for use (see links above). The certified-connectors folder is managed by the Microsoft Connector Certification Team to ensure that within the master branch, the connector version is identical to that deployed in the Power Platform.

### Setup & Use
The Power Platform Connectors have implemented Azure App Service built-in authentication and authorization support (https://docs.microsoft.com/en-us/azure/app-service/overview-authentication-authorization). Deploying the Connectors with this Auth feature differs for Azure API and our OSS FHIR Server.

### First Party Auth with Azure API for FHIR
Azure API for FHIR, when deployed via the Azure Portal supports seemless integration with Azure App Service. Users with access to Azure API for FHIR only need to login when setting up a connector to apply their RBAC's to the data connection. For instance when a user with a FHIR Data Reader role authtenticates through the Connector, the connector assumes their FHIR DATA Reader role.

### First Party Auth with OSS FHIR Server
Additional work is necessary to use First Party Auth with our FHIR instances deployed by template or script. In these cases customers must register a Resource application with an exposed API endpoint, additionally the exposed API endpoint must include an Authorized Client Application

(https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**