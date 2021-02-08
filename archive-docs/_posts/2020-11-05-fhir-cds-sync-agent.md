---
layout: post
title: "FHIR-CDS Sync Agent"
tag: architecture cloud health 
category: architecture
---

The FHIR-CDS-Sync Agent follows the Microsoft Well Architected Framework focusing on the following categories:  Availability, Data Management, Design and Implementation, Management and Monitoring, Messaging, Performance and Scalability, Resiliency and Security.  

The FHIR-CDS-Sync Agent design makes use of Azure's secure and reliable infrastructure. 

![FHIR-CDS-Sync Agent Reference Architecture](/health-architectures/assets/images/SyncAgentTechnicalDesign.png)

# Description 

## HL7 Conversion 
Most Electronic Medical Record (EMR) systems use [HL7](https://www.hl7.org/) as their internal protocol 
