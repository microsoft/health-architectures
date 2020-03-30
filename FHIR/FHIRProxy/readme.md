# Secure FHIR Proxy

Secure FHIR Proxy is an Azure Function based solution to act as an intelligent FHIR based reverse proxy.
It is integrated with Azure Active Directory to provide Role based access control.  This solution contains the following examples:
 + ProxyBase - A generic FHIR Proxy with no business logic validation and only authentication verification. You can use this sample to build your own business purpose driven proxy to perform any pre processing and post processing tasks for FHIR server calls.
 + ParticipantAccess - A sample FHIR Proxy based on ProxyBase that will filter returned patient based resources to only include Patients where you are the patient or are a "Practitioner of Record" (e.g. in a participant role and are part of the patient care team) Note: this only filters patient based resources
 + SecureLink - A administration function that links AAD Principals in roles for the FHIR server with cooresponding FHIR Resources to establish a map between AAD Role Identity and FHIR
## Architecture Overview
![Fhirproxy Arch](fhirproxy_arch.png)

## How the participant proxy works
![F H I R Proxy Seq](FHIRProxy_Seq.png)

## Deploying your own FHIR Proxy

Please note you should deploy this proxy into a tenant that you control Application Registrations, Enterprise Applications, Permissions and Role Assignments

1. [Get or Obtain a valid Azure Subscription](https://azure.microsoft.com/en-us/free/)
2. [Install Azure CLI 2.0 on Linux based System or Windows Bash Shell](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-apt?view=azure-cli-latest)
3. [Download/Clone this repo](https://github.com/microsoft/health-architectures)
4. Open a bash shell into the Azure CLI 2.0 environment
5. Switch to the FHIR/FHIRproxy subdirectory of this repo ```cd FHIR\FHIRProxy```
6. Run the deployfhirproxy.bash script and follow the prompts
7. Configure Azure Active Directory and Secure Function Access (In Progress please contact smoke jumpers for information)

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

FHIR® is the registered trademark of HL7 and is used with the permission of HL7.