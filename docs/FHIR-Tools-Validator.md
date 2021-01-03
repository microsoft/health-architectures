---
title: Validator
parent: Tools
grand_parent: FHIR
nav_order: 1
---

# FHIR Validator 

![Microsoft and FHIR](/assets/images/msft-fhir.png)

Validation of resources against any profile IGs provided is also supported. It is based on the [FHIR Validator from HL7](http://hl7.org/fhir/validator/)

Validating a resource means, checking that the following aspects of the resource are valid:
- Structure: Check that all the content in the resource is described by the specification, and nothing extra is present
- Cardinality: Check that the cardinality of all properties is correct (min & max)
- Value Domains: Check that the values of all properties conform to the rules for the specified types (including checking that enumerated codes are valid)
- Coding/CodeableConcept bindings: Check that codes/displays provided in the Coding/CodeableConcept types are valid
- Invariants: Check that the invariants (co-occurrence rules, etc.) have been followed correctly
- Profiles: Check that any rules in profiles have been followed (including those listed in the Resource.meta.profile, or in - CapabilityStatement, or in an ImplementationGuide, or otherwise required by context)
- Questionnaires: Check that a QuestionnaireResponse is valid against its matching Questionnaire
- Business Rules: Business rules are made outside the specification, such as checking for duplicates, checking that references resolve, checking that a user is authorized to do what they want to do, etc.

### Deploying your own FHIR Validator
FHIR Validator provides a Docker based command line and web service endpoint to validate FHIR Resources.  Deployment options include:
- Deploy locally using Docker
- Deploy on an Azure VM using Docker
- Deploy as an Azure Container Instance 

### Download Code / Source 
FHIR Validator is located on [GitHub](https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRValidator)


### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**