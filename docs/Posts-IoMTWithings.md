---
title: Withings with IoMT for FHIR
author: Dale
parent: Posts
nav_order: 1
---

# Ingesting Withings IoMT Data with FHIR 
By Healthcare Next Horizon-3 Team 

### Introduction 
The healthcare industry is rapidly transforming due to the digitization of healthcare data. In order to meet the needs of the healthcare industry, the emerging standard of FHIR (Fast Healthcare Interoperability Resources) was created. FHIR enables a robust, extensible data model with standardized semantics and data exchange that enables all systems using FHIR to work together. This innovation when combined with Internet of Medical Things (IoMT) telemetry data (read: Fitbit, Garmin, Accu-Check health data) has the potential to transform preventative and diagnostic healthcare.  

Withings is a French medical devices company which targets consumers. They produce various connected devices such as Internet-connected: scales, blood pressure monitors, sleep analysis devices, etc. Withings provides several REST APIs that enable 3rd parties to engineer applications that can query and use data obtained from their devices. 

Azure API for FHIR is an application that enables the rapid exchange of data through Fast Healthcare Interoperability Resources (FHIR) APIs. Additionally, Microsoft now provides a managed Platform-as-a Service (PaaS) cloud offering. This is especially useful as it requires significantly less routine management and upkeep (as that is done automatically by Microsoft). 

This document outlines our solution which showcases how to connect proprietary IoMT devices and ingest their data into a managed FHIR service. 

### Proposed Solution 
This solution utilizes Withings devices as an example of an IoMT device that exposes proprietary device data and supports IoMT device integration. To utilize Withings APIs, Withings requires OAUTH authentication couple with verifiable user consent in order to utilize their data. After successfully authenticating, and once consent has been obtained, the Withings API generates authorization and refresh tokens. These tokens are then stored in token storage (Azure KeyVault) and consent storage (Azure CosmosDB) for future use.  

The backend service architecture supports two types of data ingestion: direct request via the Withings REST API, and a data push to our endpoint from the Withings notification API. In both cases, ingesting data uses a background worker (Azure functions) to receive data in Withings proprietary JSON format and then subsequently convert it to the appropriate FHIR format where it is then sent to a FHIR server (Azure API for FHIR). Once stored, this data can be used by different consumers, such as: mobile and web applications, machine learning, etc. 

Figure 1: IoMT Ingestion and Backend Application 

<a href="https://raw.githubusercontent.com/daemel/site/master/assets/images/IoMT-Withings.png" target="_blank"> <img src="https://raw.githubusercontent.com/daemel/site/master/assets/images/IoMT-Withings.png" alt="image"/></a>


### Conclusion 
The Healthcare Next Horizon-3 Team believes that this architecture reflects all current best practices with regards to cloud architecture within the Microsoft Azure cloud. It was engineered to be as simple as possible, scalable (this architecture can be distributed and enhanced later when needed), and robust. 

### References 
- Withings API: [Documentation](https://developer.withings.com/) 
- Azure FHIR: [Documentation](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/)  
- Withings IoMT Repo on GitHub: [Code](https://github.com/microsoft/health-architectures/tree/master/Internet-Of-Things-IoT/Withings-To-IoT)

### Questions, comments or needing help, please feel free to email us at: HealthArchitectures@microsoft.com  