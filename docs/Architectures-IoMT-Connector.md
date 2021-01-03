---
title: IoMT FHIR Connector
parent: IoMT for FHIR
grand_parent: Architectures
nav_order: 2
---

# IoMT FHIR Connector for Azure Reference Architecture
The IoMT Connector for Azure enables IoT devices seamless integration with FHIR services. This reference architecture is designed to accelerate adoption of IoMT projects. This solution utilizes Azure DataBricks for the ML compute. However, Azure ML Services with Kubernetes or a partner ML solution could fit into the Machine Learning Scoring Environment.

The 4 line colors show the different parts of the data journey.
- Blue = IoT data to FHIR service.
- Green = data path for scoring IoT data
- Red = Hot path for data to inform clinicians of patient risk. The goal of the hot path is to be as close to real-time as possible.
- Orange = Warm path for data. Still supporting clinicians in patient care. Data requests are typically triggered manually or on a refresh schedule.

![IoMTReference](/assets/images/IoMTReference.jpg)

### Data Ingest – Number 1 through 5
1. Data from IoT device or via device gateway sent to Azure IoT Hub/Azure IoT Edge.
2. Data from Azure IoT Edge sent to Azure IoT Hub.
3. Copy of raw IoT device data sent to a secure storage environment for device administration.
4. PHI IoMT payload moves from Azure IoT Hub to the IoMT FHIR connector for Azure. This is an OSS connector. Multiple Azure services are represented by 1 IoMT FHIR connector for Azure Icon.
5. Three parts to number 5: a. IoMT connector request Patient resource from FHIR Server. b. FHIR Server send Patient resource back to IoMT connector. c. IoMT Patient Observation is record in FHIR server.

### Machine Learning and AI Data Route – Steps 6 through 11
6. Normalized ungrouped data stream sent to Azure Function (ML Input).
7. Azure Function (ML Input) requests Patient resource to merge with IoMT payload.
8. IoMT payload with PHI is sent to Event Hub for distribution to Machine Learning compute and storage.
9. PHI IoMT payload is sent to Azure Data Lake Storage Gen 2 for scoring observation over longer time windows.
10. PHI IoMT payload is sent to Azure DataBricks for windowing, data fitting, and data scoring.
11. The Azure DataBricks requests additional patient data from data lake as needed. a. Azure DataBricks also sends a copy of the scored data to the data lake.

### Notification and Care Coordination – Steps 12 - 18
#### Hot Path
12. Azure DataBricks sends a payload to an Azure Function (ML Output).
13. RiskAssessment and/or Flag resource submitted to FHIR server. a. For each observation window a RiskAssessment resource will be submitted to the FHIR server. b. For observation windows where the risk assessment is outside the acceptable range a Flag resource should also be submitted to the FHIR server
14. Scored data sent to data repository for routing to appropriate care team. Azure SQL Server is the data repository used in this design because of its native interaction with Power BI.
15. Power BI Dashboard is updated with Risk Assessment output in under 15 minutes.

#### Warm Path
16. Power BI refreshes dashboard on data refresh schedule. Typically, longer than 15 minutes between refreshes.
17. Populate Care Team app with current data.
18. Care Coordination through Microsoft Teams for Healthcare Patient App.

### More Information
- [IoMT FHIR Connector for Azure sandbox](https://github.com/microsoft/iomt-fhir/blob/master/docs/Sandbox.md)
- [Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/)
- [Blog: Accelerate IoMT on FHIR with new Microsoft OSS Connector](https://azure.microsoft.com/en-us/blog/accelerate-iomt-on-fhir-with-new-microsoft-oss-connector/)
