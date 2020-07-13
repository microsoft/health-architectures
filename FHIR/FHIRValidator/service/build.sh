#!/bin/sh
mvn install:install-file -Dfile=./mylib/org.hl7.fhir.validation.cli-5.0.1.jar -DgroupId=org.hl7.fhir.validation.cli -DartifactId=fhir-validation-cli -Dversion=5.0.1 -Dpackaging=jar
mvn install
sh ./target/bin/webapp
