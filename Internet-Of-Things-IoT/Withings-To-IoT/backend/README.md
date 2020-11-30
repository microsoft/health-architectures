# Withings IoMT backend

API and background processes for the H3 platform.

## Table of Contents

- [Features](#features)
- [Built with](#built-with)
- [Getting started](#getting-started)
  - [Clone the mono-repo](#clone-the-mono-repo)
  - [Install project dependencies](#install-project-dependencies)
  - [Replace environment variables](#replace-environment-variables)
  - [Start the development server](#start-the-development-server)
  - [Run the end-to-end tests](#run-the-end-to-end-tests)
  - [Code quality](#code-quality)
    - [Linting](#linting)
  - [Deploying](#deploying)
- [Links](#links)

## Features

- Integration with [Withings IoMT data](http://developer.withings.com/oauth2/#section/Introduction)
- IoMT data ingestion and normalization in [FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/overview)
- Fully documented API via [Swagger](https://swagger.io/docs/specification/2-0/what-is-swagger/)

## Built with

- [Azure Active Directory B2C](https://azure.microsoft.com/en-us/services/active-directory/external-identities/b2c/)
- [Azure KeyVault](https://docs.microsoft.com/en-us/azure/key-vault/general/overview)
- [Azure CosmosDB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction)
- [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
- [Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview)
- [Azure ServiceBus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)
- [Azure Event Hubs](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about)
- [Azure Notification Hubs](https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-push-notification-overview)
- [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview)
- [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview)

## Getting started

To clone and run this application, you will need [Git](https://git-scm.com/), [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell), [DotNet Core](https://docs.microsoft.com/en-us/dotnet/core/install/), [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) installed on your computer. Opening the project in [VSCode](https://code.visualstudio.com/) using the provided [devcontainer](https://code.visualstudio.com/docs/remote/containers) will automatically set up an environment that contains all the required tools.

### Clone the mono-repo

To get started you will need to clone the H3 mono-repo to your machine. You can do this from your terminal by running:

```sh
# Clone the mono-repo
$ git clone https://healthnext@dev.azure.com/healthnext/WBA-H3/_git/WBA-H3
```

Alternately, you can use the [tooling provide in Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio) to clone the repository using your preferred IDE.

### Install project dependencies

After cloning the mono-repo, switch to the backend directory and install the project dependencies by running:

```sh
# Switch to the backend directory
cd backend

# Install API and CLI dependencies
dotnet restore
```

```sh
# Switch to the end-to-end test directory
cd backend/test

# Install end-to-end test dependencies
npm install
```

Additionally, the backend relies on many Azure services. To set up your own instances of these services, use the deployment script:

```sh
# Connect to Azure
az login

# Switch to the infrastructure directory
cd deployment

# Set up instances of all Azure services and store connection information
pwsh ./deploy.ps1 \
  -AppName "<enter some app name here>" \
  -SaveLocalSettings
```

### Replace environment variables

Environment variables are used to configure the needed dependencies. Search code/scripts and replace "REPLACE_ME" with corresponding values

### Start the development server

To start the local development server, from your terminal run:

```sh
# Switch to the API directory
cd backend/api

# Start the local development server
func host start --pause-on-error
```

This will start the Azure Functions application in [local mode](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local). Once the server has started, the API can be accessed at [http://localhost:8888](http://localhost:8888/swagger.json). To explore the capabilities of the API, consider installing [Postman](https://www.postman.com/product/api-client/) and leveraging the [API collection](./api.postman_collection.json).

> To ensure that all background processes get executed locally, remember to navigate to the Azure Functions resource and disable the `Orchestrator` and `ServiceBus` triggered functions in the Azure Portal.

### Run the end-to-end tests

To execute end-to-end tests against a running API, from your terminal run:

```sh
# Switch to the end-to-end test directory
cd backend/test

# Configure the test environment
export API_ENDPOINT="http://localhost:8888"
export B2C_PASSWORD="<input password for your Azure Active Directory B2C account here>"
export B2C_USERNAME="<input password for your Azure Active Directory B2C account here>"
export WITHINGS_PASSWORD="<input password for your Withings account here>"
export WITHINGS_USERNAME="<input user name for your Withings account here>"
export WITHINGS_USER_ID="<input user id of your Withings account here>"

# Run end-to-end test
npm test
```

### Code quality

#### Linting

This project utilizes [ESLint](https://eslint.org/), [Prettier](https://prettier.io/) and [StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) to perform code linting and formatting. Most IDEs will recognize this and highlighting linting errors within the editor experience, however linting can be manually performed by running:

```sh
# Switch to the API directory
cd backend/api

# Perform a build which includes a lint check
dotnet build
```

```sh
# Switch to the CLI directory
cd backend/cli

# Perform a build which includes a lint check
dotnet build
```

```sh
# Switch to the end-to-end test directory
cd backend/test

# Perform a lint check
npm run lint
```

### Deploying

Deployments occur automatically (via the configured [Azure DevOps Pipelines](https://dev.azure.com/healthnext/WBA-H3/_build)) when code changes are merged into the `main` branch. If you need to deploy a build manually however, you can use the API deployment script:

```sh
# Switch to the infrastructure directory
cd deployment

# Deploy only the Azure Functions code
pwsh ./deploy-api.ps1 \
  -BuildId "<enter some identifier for the deployment here>" \
  -ResourceGroupName "<enter some app name here>" \
  -DeploymentName "resources"
```

