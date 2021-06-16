# HL7 Ingest, Conversion Samples
This project contains fully functional sample deployments of ingest and conversion workflows powered by Microsoft's FHIR Converter. The goal of this project is to provide quick start examples that can be used to accelerate implementation of health data ingest into Azure Health Data Platform.  

The [FHIR Converter](https://github.com/microsoft/FHIR-Converter) is an open source project that enables healthcare organizations to convert legacy data (currently HL7 v2 messages) into FHIR bundles. Converting legacy data to FHIR expands the use cases for health data and enables interoperability.  


## Overview
This repo provides reference architecture and sample deployments for the ingest and conversion to FHIR of HL7 messages and the de-identification of FHIR resources.
These samples demonstrate incorporating Microsoft's FHIR Converter into your enterprise HL7 messaging/FHIR infrastructure to enable end-to-end workflows for a variety of use cases.
The examples include:
  + An HL7 ingest platform to consume HL7 Messages via MLLP and securely Transfer them to Azure via HL7overHTTPS and place in blob storage and produce a consumable event on a high speed ordered service bus for processing.  
  
  + A workflow that performs orderly conversion from HL7 to FHIR via the conversion API and persists the message into a FHIR Server and publishes change events referencing FHIR resources to a high speed event hub to interested subscribers.  
    

## HL7 Ingest

Infrastructure deployment that will allow you to:
1. Consume MLLP HL7 Messages
2. Securely transfer them to Azure via [HL7overHTTPS](https://hapifhir.github.io/hapi-hl7v2/hapi-hl7overhttp/specification.html)
3. Place in blob storage for audit/errors
4. Produce a consumable event on a high speed ordered service bus for processing


![Converter Ingest](hl7ingest1.png)
### <a name="ingest"></a>Deploying your own HL7 Ingest Platform
1. [Get or Obtain a valid Azure Subscription](https://azure.microsoft.com/en-us/free/)
2. [Open Azure Cloud Shell](https://shell.azure.com) you can also access this from [azure portal](https://portal.azure.com)
3. Select Bash Shell 
4. Clone this repo ```git clone https://github.com/microsoft/health-architectures```
5. Switch to the HL7Conversion subdirectory of this repo ```cd HL7Conversion```
6. Run the deployhl7ingest.bash script and follow the prompts
7. Send in an hl7 message via HL7 over HTTPS:
    + Locate the sample message samplemsg.hl7 in the root directory of the repo
    + Use a text editor to see contents
    + From the bash command shell run the following command to test the hl7overhttps ingest
      ```
        curl --trace-ascii - -H "Content-Type:text/plain" --data-binary @samplemsg.hl7 <your ingest host name from above>/api/hl7ingest?code=<your ingest host key from above>
      ``` 
    + You should receive back an HL7 ACK message
    + Congratulations!!! The sample hl7 message was accepted securely stored into blob storage and queued for further ingest processing on the deployed service bus queue
8. Send in HL7 messages using the local HL7 MLLP Relay. To run a local copy of the HL7 MLLP Relay:
    + Make sure [Docker](https://www.docker.com/) is installed and running in your shell or local client environment
    + If you are running from a local client you will need to clone this repo to the local client 
    + From a command prompt run the runhl7relay.bash(linux) or runhl7relay.cmd(windows) passing in the hl7ingest Function App URL (Saved from Above) and the function app access key (Saved from above) as parameters.
        ```
        runhl7relay https://<your ingest host name from above/api/hl7ingest "<function app key from above>"
       ``` 
    + You can now point any HL7 MLLP Engine to the HL7 Relay listening port (default is 8079) and it will transfer messages to the hl7ingest function app over https
    + An appropriate HL7 ACK will be sent to the engine from the relay listener

## HL7toFHIR Conversion

Infrastructure deployment that will create a logic app based workflow that performs orderly conversion from HL7 to FHIR via the [FHIR Converter](https://github.com/microsoft/FHIR-Converter), persists the message into an [Azure API for FHIR Server Instance](https://azure.microsoft.com/en-us/services/azure-api-for-fhir/).

Features of the HL7toFHIR Conversion Platform:
  +  Highly Scalable, Secure and Flexible conversion process implementation
  +  Custom Connectors to the FHIR Converter and FHIR Server via the secure FHIR Proxy for easy access to these resources via logic app workflows
  +  You can also provide a central ingest point and event bus to support virtually unlimited event driven workflow and orchestration scenarios by enabling the [PublishFHIREventPostProcess module of the FHIR Proxy](https://github.com/microsoft/fhir-proxy#publish-event-post-processor)
![Converter Ingest](hl72fhir.png)

### <a name="convert"></a> Deploying your own HL7toFHIR Conversion Workflow
1. [Deploy the HL7 Ingest Platform](#ingest)
2. [Deploy the FHIR Proxy](https://github.com/microsoft/fhir-proxy)
3. [Enable the TransformBundlePreProcess on the FHIRProxy](https://github.com/microsoft/fhir-proxy#transform-bundle-pre-processor)
4. [Place a user account, group or service principal in the appropriate access role for the FHIR Proxy](https://github.com/microsoft/fhir-proxy#adding-usersgroups-to-the-fhir-server-proxy)
   This account will be used to allow logic app connectors and cooresponding orchestrations access to FHIR Services via the FHIR Proxy client
5. [Open Azure Cloud Shell](https://shell.azure.com) you can also access this from [azure portal](https://portal.azure.com)
6. Select Bash Shell 
7. Clone this repo (if needed) ```git clone https://github.com/microsoft/health-architectures```
8. Switch to the HL7Conversion subdirectory of this repo
9. Run the deployhl72fhir.bash script and follow the prompts
10. Authenticate your FHIR Proxy Custom Connector
   + [Access Azure Portal](https://portal.azure.com)
   + Goto the resource group that contains the deployed HL7toFHIR workflow
   + Find the FHIRServerProxy-1 API Connection and click on it
   + Click on Edit API Connection
   + Enter a Display name of FHIR Server Proxy
   + Click Authorize
   + Enter the Credentials for the user account, group or service principal from step #4
   + Click Save
   + Click Overview make sure status is Connected 
11. After successful deployment your converter pipeline is now tied to your ingest platform from above.  To test simply follow the test direction for HL7 Ingest above with the sample HL7 message and you should see resources from the bundle created in the destination FHIR Server
   + You can also see execution from the HL7toFHIR Logic App Run History in the HL7toFHIR resource group.  This will also provide you with detailed steps to see the transform process:

![Sample Converter Run](samplerun.png)


FHIR� is the registered trademark of HL7 and is used with the permission of HL7</br>
HAPI is an open source tool set developed by [University Health Network](http://www.uhn.ca/)
