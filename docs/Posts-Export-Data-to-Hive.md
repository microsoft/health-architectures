---
title: Export Data to Hive, HDFS or Azure Data Bricks
author: Cory
parent: Posts
nav_order: 1
---

# Exporting Data to Hive, HDFS or Azure Data Bricks   

Last week a customer asked our team the following question, "If I want to export FHIR to Hive what is the best practice?" Turns out there is a very simple method to accomplish this task. This walkthrough post works with Hive, Hadoop, Spark, DataBricks or any other tools supporting the HDFS data sources.

To understand more on how Azure Data Lake Gen2 supports HDFS storage check out "Introduction to Azure Data Lake Storage Gen2" link at the bottom of this post.

The following list are items we assume your organization as adopted prior to this setup:

1. The organization has an active Azure subscription.
2. The organization has already set up permissions, access controls and networks in Azure. The export of FHIR data deals with PHI/ PII data.
3. The organization already has setup a FHIR service with data. We are using the Azure API for FHIR. Most FHIR services supporting $export will be able to follow this method.
4. The organization is following a 'schema on read' methodology.
The key to exporting to Hive is configuring the export storage account to point to Azure Data Lake Gen2. To accomplish the setup and automation of the exporting data from FHIR to Azure Data Lake Store Gen2 for use by Hive you will need access to the Azure portal.

We will walk through part of the instructions in this post. I encourage you to finish reading this post prior to jumping to the full instructions. This way you know the keys to a successful configuration. The link to the API for FHIR Integrations instructions are at the bottom of this post.

After opening the Azure portal and navigating to your Azure API for FHIR instance, you should see the Integration tab about halfway down on the left.

![FHIR Integration Button](/assets/images/IntegrationButtonHighlight.png)

After you open the Integrations blade on your Azure API for FHIR, you will notice the storage account type is not listed on the table displaying a list of available storage accounts. You will see Name, Resource Group and Region but not Storage Account Type. Therefore, you will need to know your Azure Data Lake Gen2 account name prior to setting up the Integration.

![FHIR Storage Account Type](/assets/images/FHIRExportStorageAccountType.png)

If you do know have an Azure Data Lake Gen2 account go set one up and come back to this screen. Once you have set up an Azure Data Lake Gen2 account record the name for use on the API for FHIR Integrations blade. Back on the Integrations blade type, paste or choose the Data Lake storage account name into the Export Storage Account box.

![FHIR Export Storage Box](/assets/images/ExportStorageAccountBox.png)

Technically you have completed everything necessary for exporting to Hive or an HDFS Storage location. However, I have never met a customer who wants to export data for research or analytics who exports once. Because most customers need to export data on a regular basis our team has created the FHIR Export Quickstart for automating the export.

The FHIR Export Quickstart configuration is as simple as setting up the Azure Data Lake Gen2 connection. This template will automate bulk export data from the Azure API for FHIR to Azure Data Lake Gen2. You can find more details in the GitHub Repo [here.](https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRExportQuickstart)

## Quick Review

1. You have an Azure API for FHIR setup with data in the system.
2. You created an Azure Data Lake Gen2 storage account. Note the name. If you already have a Data Lake storage account setup, skip this step, but note the name.
3. In the Azure portal, navigate to your Azure API for FHIR service. Find the Integration blade. In the Export Storage Account box enter or choose the name of your Azure Data Lake Gen 2. Click Save.
4. Still in the portal on the Azure API for FHIR, click the Identity blade. Make sure status is set to 'On'. Note the Object ID and click save if you turned the status to 'On'.
5. Make sure to grant your Azure API for FHIR rights to the Azure Data Lake Gen2 account. We did not cover these steps above. Here is the link to the instructions <https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data#adding-permission-to-storage-account>
6. Setup the FHIRExportQuickstart Logic App to automate the export data process.

As promised the links:

- Setting up the Azure API for FHIR Integration - <https://docs.microsoft.com/en-us/azure/healthcare-apis/configure-export-data>
- FHIR Export Quickstart -<https://github.com/microsoft/health-architectures/tree/master/FHIR/FHIRExportQuickstart>
- Introduction to Azure Data Lake Storage Gen2 <https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-introduction>.
- Connecting Azure Data Lake Gen2 to Hive/Hadoop - <https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-tutorial-extract-transform-load-hive>
- Connecting Azure Data Lake Gen2 to Azure DataBricks Tutorial - <https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-use-databricks-spark>

For more Microsoft Health Data and Cloud architectures, quickstarts, and samples check out <https://aka.ms/HealthArchitectures>

For Information on Azure API for FHIR <https://docs.microsoft.com/en-us/azure/healthcare-apis/>

Acronym References:

- PHI - Protected Health Information [reference](https://en.wikipedia.org/wiki/Protected_health_information)
- PII - Patient Identifiable Information [reference](https://www.investopedia.com/terms/p/personally-identifiable-information-pii.asp)
