# FHIR Event Processor
# !!!DEPRECATED!!! This functionality has been incorporated into the [FHIR Proxy](https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRProxy)
# This code is no longer maintained
FHIR Event Processor is an Azure Function App solution that provides the following services for ingesting FHIR Resources into the FHIR Server:
 + Import and process valid HL7 bundles and persist them into a FHIR Compliant store.
 + Provides a secure proxy a connection to a destination FHIR Server without exposing credentials
 + Publish successful FHIR Server CRUD events to an event hub for topic subscribers to use in event driven workflows

