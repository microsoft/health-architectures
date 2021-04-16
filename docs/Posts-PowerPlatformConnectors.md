---
title: Power Platform Connectors
author: Dale
parent: Posts
nav_order: 1
---

# Power Platform Connectors for Azure API for FHIR

A connector is a proxy or a wrapper around an API that allows the underlying service to talk to Microsoft Power Platform.  Connectors provide a way for users to leverage a set of pre-built actions and triggers to build applications and workflows.

Two Connectors are being deployed as part of Microsoft Cloud for Healthcare, each works with Azure API for FHIR and OSS FHIR Server.  The connectors contain a subset of FHIR Resources and are bi-directional supporting both reads and writes to the FHIR Service.  Currently these connectors are certified for Power Apps and Power Automate, Logic Apps will be evaluated should there be interest.  

![Connector Overview](/assets/images/ConnectorConcept.png)

## Why Create a set of Power Platform FHIR Connectors

The healthcare industry is rapidly transforming health data to the emerging standard of FHIR (Fast Healthcare Interoperability Resources). FHIR enables a robust, extensible data model with standardized semantics and data exchange that enables all systems using FHIR to work together.  By creating Low to No Code connectors for FHIR, we are opening up FHIR to citizen developers and front line workers, enabling them to create solutions that meet their needs in real time. 

##  Feedback Requested 

Questions, comments or needing help, please feel free to email us at HealthArchitectures@microsoft.com

## Supported FHIR Resources 

The follow FHIR Resources are supported at launch 

| Connector Name	| Operation	| Description	|
|---	|---	|---	|
| FHIRBase	| Appointment	| A booking of a healthcare event among patient(s), practitioner(s), related person(s) and/or device(s) for a specific date/time	|
| FHIRBase	| AppointmentResponse	| A reply to an appointment request for a patient and/or practitioner(s), such as a confirmation or rejection	|
| FHIRBase	| Device	| A type of a manufactured item that is used in the provision of healthcare without being substantially changed through that activity. The device may be a medical or non-medical device	|
| FHIRBase	| Encounter	| An interaction between a patient and healthcare provider(s) for the purpose of providing healthcare service(s) or assessing the health status of a patient	|
| FHIRBase	| Flag	| Prospective warnings of potential issues when providing care to the patient	|
| FHIRBase	| Location	| Details and position information for a physical place where services are provided and resources and participants may be stored, found, contained, or accommodated	|
| FHIRBase	| Patient	| Demographics and other administrative information about an individual or animal receiving care or other health-related services	|
| FHIRBase	| Person	| Demographics and administrative information about a person independent of a specific health-related context	|
| FHIRBase	| Practitioner	| A person who is directly or indirectly involved in the provisioning of healthcare	|
| FHIRClinical	| AdverseEvent	| Actual or potential/avoided event causing unintended physical injury resulting from or contributed to by medical care, a research study or other healthcare setting factors that requires additional monitoring, treatment, or hospitalization, or that results in death	|
| FHIRClinical	| AllergyIntolerance	| Risk of harmful or undesirable, physiological response which is unique to an individual and associated with exposure to a substance	|
| FHIRClinical	| CarePlan	| Describes the intention of how one or more practitioners intend to deliver care for a particular patient, group or community for a period of time, possibly limited to care for a specific condition or set of conditions	|
| FHIRClinical	| CareTeam	| The Care Team includes all the people and organizations who plan to participate in the coordination and delivery of care for a patient	|
 | FHIRClinical	| Condition	| A clinical condition, problem, diagnosis, or other event, situation, issue, or clinical concept that has risen to a level of concern	|
| FHIRClinical	| DiagnosticReport	| The findings and interpretation of diagnostic tests performed on patients, groups of patients, devices, and locations, and/or specimens derived from these. The report includes clinical context such as requesting and provider information, and some mix of atomic results, images, textual and coded interpretations, and formatted representation of diagnostic reports	|
| FHIRClinical	| Medication	| This resource is primarily used for the identification and definition of a medication for the purposes of prescribing, dispensing, and administering a medication as well as for making statements about medication use	|
| FHIRClinical	| MedicationRequest	| An order or request for both supply of the medication and the instructions for administration of the medication to a patient. The resource is called "MedicationRequest" rather than "MedicationPrescription" or "MedicationOrder" to generalize the use across inpatient and outpatient settings, including care plans, etc., and to harmonize with workflow patterns	|
| FHIRClinical	| MedicationStatement	| A record of a medication that is being consumed by a patient. A MedicationStatement may indicate that the patient may be taking the medication now or has taken the medication in the past or will be taking the medication in the future	|
| FHIRClinical	| Observation	| Measurements and simple assertions made about a patient, device or other subject	|
| FHIRClinical	| Procedure	| An action that is or was performed on or for a patient. This can be a physical intervention like an operation, or less invasive like long term services, counseling, or hypnotherapy	|
| FHIRClinical	| RiskAssessment	| An assessment of the likely outcome(s) for a patient or other subject as well as the likelihood of each outcome	|

