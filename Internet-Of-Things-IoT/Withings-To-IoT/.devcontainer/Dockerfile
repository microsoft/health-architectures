# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

FROM ubuntu:18.04

RUN apt-get update \
 && apt-get install -y curl apt-transport-https \
 && curl -fsSLO https://deb.nodesource.com/setup_12.x \
 && bash setup_12.x \
 && rm setup_12.x \
 && curl -fsSLO https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb \
 && dpkg -i packages-microsoft-prod.deb \
 && rm packages-microsoft-prod.deb \
 && apt-get update \
 && apt-get install -y \
        azure-cli \
        azure-functions-core-tools-3 \
        dotnet-sdk-3.1 \
        git \
        nodejs \
        powershell \
        python3 \
        python3-pip

COPY deployment/install-dependencies.ps1 /tmp/
RUN pwsh /tmp/install-dependencies.ps1

COPY ml/requirements-dev.txt /tmp/
RUN python3 -m pip install -r /tmp/requirements-dev.txt

ENV LC_ALL=C.UTF-8
ENV LANG=C.UTF-8
