# FHIR Validator

FHIR Validator provides an Docker based command line and web service endpoint to validate FHIR Resources. Validation of resources against any profile IGs provided is also supported. It is based on the [FHIR Validator from HL7](http://hl7.org/fhir/validator/)  

## Deploying your own FHIR Validator

Make sure [Docker](https://www.docker.com/) is installed and running in your linux or windows environment

## CLI
To use the Command Line Interface
1. Switch to the ```fhirvalidator/cli``` sub-directory of this repo
2. Run the following command file depending on your OS:
</br>for Windows:
```fhirvalidator.cmd```
</br>for Linux:
```./fhirvalidator.sh```

By default the ```fhirvalidator/cli``` directory will be mounted as ```localfiles``` in the container. You can place FHIR Resource files to be validated into the local ```fhirvalidator/cli``` directory and they can be accessed using the using the
```localfiles``` mount point in the container.  You can also export results from the validator to the ```localfiles``` mount point in the container and they will be accessable in the local ```fhirvalidator/cli``` diretory
</br>For example to validate a FHIR resource file called patient.json for comformance with R4 (4.0.1) of FHIR place the patient.json file in the local ```fhirvalidator/cli``` directory and issue the following command in the container bash shell of the container:
</br></br>```java -jar org.hl7.fhir.validation.cli-5.0.1.jar /localfiles/patient.json -version 4.0.1```
</br></br>
Please see the [HL7 FHIR Validator documentation](https://wiki.hl7.org/Using_the_FHIR_Validator) for the CLI full documentation.

## Service End Point
To use the service endpoint:
1. Switch to the ```fhirvalidator/service``` sub-directory of this repo
2. Run the following command file depending on your OS:
</br>for Windows:
```fhirvalidator.cmd```
</br>for Linux:
```./fhirvalidator.sh```

This service endpoint will start and by default will expose the validation engine on ```localhost:8080``` The service by default will allow validation of R4 FHIR
resources and/or bundles with the public HL7 terminology services. You can optionally also validate against any of the [US Core Profiles](https://www.hl7.org/fhir/us/core/). 
</br></br>To validate an R4 resource or bundle for compliance, POST a FHIR resource or bundle in the HTTP BODY to url: ```http://localhost:8080/validate``` using your favorite HTTP/REST utility or your client..
</br></br>To validate an R4 resource or bundle and check for profile compliance against US-Core-Patient POST a FHIR Patient resource in the HTTP BODY to url: ```http://localhost:8080/validate?profile=http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient``` using your favorite HTTP/REST utility or your client.
</br>The result will be an [OperationOutcome](https://www.hl7.org/fhir/operationoutcome.html) resource with detailed error and warning information.  

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
