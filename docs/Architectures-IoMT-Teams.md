---
title: IoMT with Teams
parent: IoMT for FHIR
grand_parent: Architectures
nav_order: 2
---

# IoMT Connector for Azure and Microsoft Teams Notifications
When combining the IoMT Connector for Azure, Azure API for FHIR, and Microsoft Teams customers can enable multiple care solutions. Below is the IoMT to MS Teams Notification conceptual architecture for enabling IoMT connector, FHIR and Microsoft Teams Patient App. We can even embed Power BI Dashboards inside the Microsoft Teams client. For more information on embedding Power BI in Microsoft Team visit here.

IoMT Connector and Team Reference Architecture

![IoMTReference](/assets/images/IoMT2TeamsConcept.jpg)

The IoMT FHIR Connector for Azure can ingest IoT data from most IoT devices or gateways regardless of location, data center or cloud. We do encourage the use of Azure IoT services to assist with device/gateway connectivity.

![IoMTtoTeamsConceptwithIoTHub](/assets/images/IoMT2TeamsConceptwithHub.jpg)

For some solutions, Azure IoT Central can be used in place of Azure IoT Hub. We will be using Azure IoT Central in the sandbox setup.

Azure IoT Edge can be used in conjunction with IoT Hub to create an on-premise end point for devices and/or in-device connectivity.

![IoMT2TeamswithIoTEdge](/assets/images/IoMT2TeamswithIoTEdge.jpg)


### More Information
- [IoMT FHIR Connector for Azure sandbox](https://github.com/microsoft/iomt-fhir/blob/master/docs/Sandbox.md)
- [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/)
- [Blog: Accelerate IoMT on FHIR with new Microsoft OSS Connector](https://azure.microsoft.com/en-us/blog/accelerate-iomt-on-fhir-with-new-microsoft-oss-connector/)