#!/bin/sh
docker image rm myfhirvalidator-cli
docker build . -t myfhirvalidator-cli
docker run --rm -it -v $PWD:/localfiles myfhirvalidator-cli
