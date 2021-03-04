---
title: Validator - Deployment as a Container Instance
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

### Deploying your own FHIR Validator as a Containter Instance 
 
### Step 1 Download Health-Architectures Code 
Open cloud shell, clone git repo 
```bash
$ git clone https://github.com/microsoft/health-architectures.git
``` 

If you belong to more than one subscription, ensure you are using the correct one. 
```bash
$ az account set --subscription "subscriptionName"
```
 

### Step 2 Create a Container Registry 
Create a container registry (Portal)
Quickstart - Create registry in portal - Azure Container Registry | [Microsoft Docs](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal)
 
![Portal View](/assets/images/ContainerReg1.png)


Enable admin user on the registry
Registry authentication options - Azure Container Registry | [Microsoft Docs](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-authentication#admin-account) 
```bash
$ az acr update -n <acrName> --admin-enabled true
```

### Step 3 Load the Validator Image into the Registry 
 
Build Image from SmokeJumper repo (from the Cloud Shell) 
 
Change directory to the fhir validator service (not CLI) 
```bash
$ cd ./health-architectures/FHIR/FHIRValidator/service/
```
 
Enable shell script execution  (chmod +x ./filename.sh)
```bash
$ chmod +x ./*.sh
```
 
Build the Image from the Service Dockerfile  
 
Sample:  
```bash
$ az acr build --image <name of image in repository:tagnumber> --registry <name of the container repository> --file Dockerfile .
```
Example:
```bash 
$ az acr build --image demo/validator:v1 --registry demoregistory1 --file Dockerfile .
```
### Step 4 Create a new Container Instance
 
Create new Container Instance using image from above | [Microsoft Docs](https://docs.microsoft.com/en-us/azure/container-instances/container-instances-quickstart-portal)
 
![Portal View](/assets/images/Container-Inst1.png)

Note: There are no heap settings in the validator project as JDK 10+ projects run in a container they automatically allocate heap size for the JVM.   They should use a percentage of memory allocated to the container, as an example giving the container 4gb of memory to run in the JVM will reserve 2gb of that  (50% is default) you can change that setting as a percentage if you desire.  We recommend using a service plan instance with at least 10GB of RAM   

Note:  You must go to Next: Networking and set the TCP Port to 8080 to allow connections to the validator

![Portal View](/assets/images/container-instance-deploy.png)

### Step 5 Test 

![Portal View](/assets/images/validator-test.png)

---
### Download Code / Source 
FHIR Validator is located on [GitHub](https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRValidator)


### Support or Contact

For more information on health solutions email us **@ <a href="mailto:HealthArchitectures@microsoft.com">HealthArchitectures</a>**