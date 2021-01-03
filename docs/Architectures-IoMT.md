---
title: IoMT for FHIR
parent: Architectures
has_children: true
nav_order: 1
---

# Internet of Medical Things (IoMT)

![Microsoft and FHIR](/assets/images/msft-fhir.png)

The IoMT FHIR Connector for Azure is an open-source project for ingesting data from IoMT (Internet of Medical Things) devices and persisting the data in a FHIR server.  Device data can be written to directly to the IoMT FHIR Connector for Azure or seamlessly used in concert with other Azure IoT solutions (IoT Hub and IoT Central). The connector does not provide device security or management which are covered by the Azure IoT solutions mentioned.

The IoMT FHIR Connector for Azure is built with extensibility in mind, enabling developers to modify and extend the capabilities to support additional device mapping template types and FHIR resources. The different points for extension are:
- Normalization: Device data information is extracted into a common format for further processing.
- FHIR Conversion: Normalized and grouped data is mapped to FHIR. Observations are created or updated according to configured templates and linked to the device and patient.

