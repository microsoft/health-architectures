### IoMT Connector for Azure and Microsoft Power BI

Microsoft Power BI is a powerful business intellegence/ analytics tool which wehn combined with FHIR & IoT can change the way providers care for patients.

The conceptual design below shows the basic components of using Microsoft cloud services to enable Power BI on top of IoMT and FHIR data.
We can even embed Power BI Dashbaords inside the Microsoft Teams client to further enchance care team coordination. For more information on embeding Power BI in Microsoft Team visit [here.](https://support.office.com/en-us/article/add-a-power-bi-tab-to-teams-708ce6fe-0318-40fa-80f5-e9174f841918)

#### IoMT Connector and Power BI Reference Architecture 

![IoMTtoPBIConcept](./images/IoMT2PBIConcept.jpg)

The IoMT FHIR Connector can ingest IoT data from most IoT devices or gateways regardless of location, data center or cloud. We do encourage the use of Azure IoT services to 
assist with device/gateway connectivity. 

![IoMTtoPBIConceptwithIoTHub](./images/IoMT2PBIwIoTHub.jpg)


Azure IoT Central can be used in place of Azure IoT Hub. We will be using Azure IoT Central in the sanbox setup. 

Azure IoT Edge can be used in conjuction with IoT Hub to create an on premise end point for devices and/or in-device connectivity. 

![IoMTtoPBIConceptwithIoTEdge](./images/IoMT2PBIwithIoTEdge.jpg)


#### Steps to create sandbox environment

By following the steps below we can impliment the conceptual solution by adding to the IoMT FHIR Sandbox.

1. Setup an [IoMT FHIR Connector for Azure sandbox.](https://github.com/microsoft/iomt-fhir/blob/master/docs/Sandbox.md)
    - Make sure to note in a text editor the FHIR Service, client ID and client secret at the end of the PowerShell script. Do not close the PowerShell window. We will need it for the next step.
2. Run the following PowerShell script to two patients to the FHIR service. Follow the prompts 
```PowerShell
.\UploadSamplePatients.ps1 -EnvironmentName <ENVIRONMENTNAME> 
```
3. Run the following PowerSheel script to link the device and patients.
```PowerShell
.\LinkPatientandDevice.ps1  -EnvironmentName <ENVIRONMENTNAME>
```
4.  Open the Power BI file in ___________ and fill in the fill in ......

Once complete you should see your two patients in the Teams Patient App. 

## More Information
- [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/)
- Blog: [Accelerate IoMT on FHIR with new Microsoft OSS Connector](https://azure.microsoft.com/en-us/blog/accelerate-iomt-on-fhir-with-new-microsoft-oss-connector/)
- IoMT Complete Reference Architecture: [IoMT Solution](./IoMTReferenceArchitecture.md)
- 

