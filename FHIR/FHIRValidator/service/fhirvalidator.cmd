
docker image rm myfhirvalidator-svc
docker build . -t myfhirvalidator-svc
docker run -it --rm -p 8080:8080 myfhirvalidator-svc
