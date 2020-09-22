---
layout: default
---

# Microsoft Health Architectures 
Health Architectures is a collection of reference architectures and, when appropriate, implementations. They illustrate end-to-end best practices for using the Azure API for FHIR and related technologies.  

## Scenarios & Solutions 

### Data Ingest Scenarios & Solutions 
Electronic health records must be available and discoverable, across healthcare systems.   
- [Ingesting Medical Device Data - IoMT](https://github.com/microsoft/health-architectures/tree/master/Internet-Of-Things-IoT/IoMT-FHIR-Connector-for_Azure)
- [Ingesting HL7 Records](https://github.com/microsoft/health-architectures/tree/master/HL7Conversion)
- Ingesting FHIR Records and Bundles *(coming soon)* 

### Research and Analytics Scenarios & Soluitions   
Healthcare analytics is the systematic use of observation, encounter and care data to create meaningful insights. While FHIR supports the ability to export data, healthcare architectures focuses on de-identification and analytics through proven platforms such as [PowerBI](https://docs.microsoft.com/en-us/power-query/connectors/fhir/fhir), [DataBricks](https://azure.microsoft.com/en-us/free/databricks) and [Azure Synapse](https://azure.microsoft.com/en-us/services/synapse-analytics).  
- [Exporting Data](https://github.com/microsoft/health-architectures/tree/master/Research-and-Analytics/FHIRExportQuickstart)
- [Anonymized Data Export](https://github.com/microsoft/health-architectures/tree/master/Research-and-Analytics/FHIRExportwithAnonymization)

## Code Demo's
Microsoft Health hosts a [fhir-server-sampes respository](https://github.com/microsoft/fhir-server-samples) contains example applications and scenarios that show use of the FHIR Server for Azure and the Azure API for FHIR.


<br>


<h2>Latest Posts</h2>

<ul>
  {% for post in site.posts %}
    <li>
      <h3><a href="{{ post.url | absolute_url }}">{{ post.excerpt }} </a></h3>
    </li>
  {% endfor %}
</ul>



We invite you to ask questions, make suggestions and share use cases which we might consider for future reference architectures or implementations.
