docker image rm myfhirvalidator-cli
docker build . -t myfhirvalidator-cli
docker run --rm -it -v %~dp0:/localfiles myfhirvalidator-cli
