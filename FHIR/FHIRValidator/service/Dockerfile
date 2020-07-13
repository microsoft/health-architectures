FROM maven:3.6.3-openjdk-11
ENV PORT=8080
COPY src /src
copy lib /mylib
copy pom.xml /
copy build.sh /
WORKDIR /
EXPOSE 8080
ENTRYPOINT ["./build.sh","console"]