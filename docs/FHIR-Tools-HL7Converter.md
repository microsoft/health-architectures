---
title: HL7 Conversion
parent: Tools
grand_parent: FHIR
nav_order: 1
---

# HL7 Conversion 

![Microsoft and FHIR](/assets/images/msft-fhir.png)

### Introduction
The [FHIR Converter](https://github.com/microsoft/FHIR-Converter) is an open source project that enables healthcare organizations to convert legacy data (currently HL7 v2 messages) into FHIR bundles. Converting legacy data to FHIR expands the use cases for health data and enables interoperability.

The GitHub repo provides reference architecture and sample deployments for the ingest and conversion to FHIR of HL7 messages and the de-identification of FHIR resources. These samples demonstrate incorporating Microsoft's FHIR Converter into your enterprise HL7 messaging/FHIR infrastructure to enable end-to-end workflows for a variety of use cases. The examples include:
- An HL7 ingest platform to consume HL7 Messages via MLLP and securely Transfer them to Azure via HL7overHTTPS and place in blob storage and produce a consumable event on a high speed ordered service bus for processing.
- A workflow that performs orderly conversion from HL7 to FHIR via the conversion API and persists the message into a FHIR Server and publishes change events referencing FHIR resources to a high speed event hub to interested subscribers.

### Download Code / Source 
FHIR Validator is located on [GitHub](https://github.com/microsoft/health-architectures/tree/master/HL7Conversion)

### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**