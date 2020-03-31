### IoMT Connector for Azure and Microsoft Power BI

Microsoft Power BI is a powerful business intelligence/ analytics tool which when combined with FHIR & IoT can change the way providers care for patients.

The conceptual design below shows the basic components of using Microsoft cloud services to enable Power BI on top of IoMT and FHIR data. We can even embed Power BI Dashboards inside the Microsoft Teams client to further enhance care team coordination. For more information on embedding Power BI in Microsoft Team visit [here.](https://support.office.com/en-us/article/add-a-power-bi-tab-to-teams-708ce6fe-0318-40fa-80f5-e9174f841918)

#### IoMT Connector and Power BI Reference Architecture 

![IoMTtoPBIConcept](./images/IoMT2PBIConcept.jpg)

The IoMT FHIR Connector for Azure can ingest IoT data from most IoT devices or gateways regardless of location, data center or cloud. We do encourage the use of Azure IoT services to 
assist with device/gateway connectivity. 

![IoMTtoPBIConceptwithIoTHub](./images/IoMT2PBIwIoTHub.jpg)


For some solutions, Azure IoT Central can be used in place of Azure IoT Hub. We will be using Azure IoT Central in the sandbox setup. 

Azure IoT Edge can be used in conjunction with IoT Hub to create an on-premise end point for devices and/or in-device connectivity. 

![IoMTtoPBIConceptwithIoTEdge](./images/IoMT2PBIwithIoTEdge.jpg)


#### Steps to create sandbox environment

Steps for creating a demo environment coming soon.

In the meantime, check out [IoMT FHIR Connector for Azure sandbox.](https://github.com/microsoft/iomt-fhir/blob/master/docs/Sandbox.md). Setting this up is the first step in creating
 a sample solution so why not get started today?

## More Information
- [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/)
- [Microsoft Health](https://azure.microsoft.com/en-us/industries/healthcare/)
- Blog: [Accelerate IoMT on FHIR with new Microsoft OSS Connector](https://azure.microsoft.com/en-us/blog/accelerate-iomt-on-fhir-with-new-microsoft-oss-connector/)
- IoMT Complete Reference Architecture: [IoMT Solution](./IoMTReferenceArchitecture.md)
- More [Health References](https://github.com/microsoft/health-references)


