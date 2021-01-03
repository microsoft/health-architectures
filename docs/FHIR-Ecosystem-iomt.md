---
title: IoMT for FHIR
parent: Ecosystem
grand_parent: FHIR
nav_order: 2
---

# IoMT Connector for FHIR for FHIR 

![Microsoft and FHIR](/assets/images/msft-fhir.png)

Azure IoT Connector for Fast Healthcare Interoperability Resources (FHIR) is an optional feature of Azure API for FHIR that provides the capability to ingest data from Internet of Medical Things (IoMT) devices. 

Internet of Medical Things is a category of IoT devices that capture and exchange health & wellness data with other healthcare IT systems over network. Some examples of IoMT devices include fitness and clinical wearables, monitoring sensors, activity trackers, point of care kiosks, or even a smart pill. The Azure IoT Connector for FHIR feature enables you to quickly set up a service to ingest IoMT data into Azure API for FHIR in a scalable, secure, and compliant manner.

Azure IoT Connector for FHIR can accept any JSON-based messages sent out by an IoMT device. This data is first transformed into appropriate FHIR-based Observation resources and then persisted into Azure API for FHIR. The data transformation logic is defined through a pair of mapping templates that you configure based on your message schema and FHIR requirements. Device data can be pushed directly to Azure IoT Connector for FHIR or seamlessly used in concert with other Azure IoT solutions (Azure IoT Hub and Azure IoT Central). Azure IoT Connector for FHIR provides a secure data pipeline while allowing the Azure IoT solutions manage provisioning and maintenance of the physical devices.


### Reference Architectures 
[IoMT and FHIR](site/Architectures-IoMT.html)

### Resources 
- [Azure IoT Connector for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/overview#azure-iot-connector-for-fhir-preview)
- [Azure API for FHIR](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/)

