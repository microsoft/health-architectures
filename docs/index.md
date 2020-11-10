---
layout: default
---

# Microsoft Health Architectures 
Health Architectures is a collection of reference architectures and, where appropriate, implementations. They illustrate end-to-end best practices for using the Azure API for FHIR and related technologies.  

## Reference Architectures 
- [FHIR-CDS Sync Agent](https://microsoft.github.io/health-architectures/refarch.html)

## Data Ingest Scenarios & Solutions 
- [Ingesting Medical Device Data - IoMT](https://github.com/microsoft/health-architectures/tree/master/Internet-Of-Things-IoT/IoMT-FHIR-Connector-for_Azure)
- [Ingesting HL7 Records](https://github.com/microsoft/health-architectures/tree/master/HL7Conversion)
- Ingesting FHIR Records and Bundles *(coming soon)* 

## Research and Analytics Scenarios & Soluitions   
- [Exporting Data](https://github.com/microsoft/health-architectures/tree/master/Research-and-Analytics/FHIRExportQuickstart)
- [Anonymized Data Export](https://github.com/microsoft/health-architectures/tree/master/Research-and-Analytics/FHIRExportwithAnonymization)

## Microsoft Cloud for Healthcare  
 - [Low Code Development](https://github.com/microsoft/health-architectures/tree/master/Low-Code) 


<h2>Latest Posts</h2>


<ul>
  {% for post in site.posts %}
    <li>
      <h3><a href="{{ post.url| absolute_url }}">{{ post.title }}</a></h3>
    </li>
  {% endfor %}
</ul>



We invite you to ask questions, make suggestions and share use cases which we might consider for future reference architectures or implementations.

v2