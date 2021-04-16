---
title: IoMT with PowerBI
parent: IoMT for FHIR
grand_parent: Architectures
nav_order: 2
---

# IoMT Connector for Azure and Microsoft Power BI

Microsoft Power BI is a powerful business intelligence/ analytics tool which when combined with FHIR & IoT can change the way providers care for patients.

The conceptual design below shows the basic components of using Microsoft cloud services to enable Power BI on top of IoMT and FHIR data. We can even embed Power BI Dashboards inside the Microsoft Teams client to further enhance care team coordination. For more information on embedding Power BI in Microsoft Team visit [here](https://docs.microsoft.com/en-us/power-bi/collaborate-share/service-embed-report-microsoft-teams).

### IoMT Connector and Power BI Reference Architecture

![IoMT Connector and Power BI Reference Architecture](/assets/images/IoMT2PBIConcept.jpg)

The IoMT FHIR Connector for Azure can ingest IoT data from most IoT devices or gateways regardless of location, data center or cloud. We do encourage the use of Azure IoT services to assist with device/gateway connectivity.

![IoMT Connector and Power BI Reference Architecture](/assets/images/IoMT2PBIwIoTHub.jpg)

For some solutions, Azure IoT Central can be used in place of Azure IoT Hub. We will be using Azure IoT Central in the sandbox setup.

Azure IoT Edge can be used in conjunction with IoT Hub to create an on-premise end point for devices and/or in-device connectivity.

![IoMT Connector and Power BI Reference Architecture](/assets/images/IoMT2PBIwithIoTEdge.jpg)


### More Information
- [IoMT FHIR Connector for Azure sandbox](https://github.com/microsoft/iomt-fhir/blob/master/docs/Sandbox.md)
- [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/)
- [Blog: Accelerate IoMT on FHIR with new Microsoft OSS Connector](https://azure.microsoft.com/en-us/blog/accelerate-iomt-on-fhir-with-new-microsoft-oss-connector/)

