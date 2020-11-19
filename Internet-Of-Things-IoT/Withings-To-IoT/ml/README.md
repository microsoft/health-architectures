# H3 ML

Machine learning, analytics and big data processing for the H3 platform.

## Table of Contents

- [Features](#features)
- [Built with](#built-with)
- [Getting started](#getting-started)
  - [Clone the mono-repo](#clone-the-mono-repo)
  - [Install project dependencies](#install-project-dependencies)
  - [Interacting with Databricks](#interacting-with-databricks)
  - [Code quality](#code-quality)
    - [Linting](#linting)
  - [Deploying](#deploying)
- [Links](#links)

## Features

- Automated ingestion of data from the [H3 backend](../backend/README.md)
- Normalization of data on a schedule
- Deletion of all user data on request

## Built with

- [Azure Event Hubs capture](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview)
- [Azure Databricks](https://docs.microsoft.com/en-us/azure/databricks/scenarios/what-is-azure-databricks)

## Getting started

To clone and run this application, you will need [Git](https://git-scm.com/), [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell), [Python](https://www.python.org/downloads/) and [Databricks CLI](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/) installed on your computer. Opening the project in [VSCode](https://code.visualstudio.com/) using the provided [devcontainer](https://code.visualstudio.com/docs/remote/containers) will automatically set up an environment that contains all the required tools.

### Clone the mono-repo

To get started you will need to clone the H3 mono-repo to your machine. You can do this from your terminal by running:

```sh
# Clone the mono-repo
$ git clone https://healthnext@dev.azure.com/healthnext/WBA-H3/_git/WBA-H3
```

Alternately, you can use the [tooling provide in Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio) to clone the repository using your preferred IDE.

### Install project dependencies

After cloning the mono-repo, switch to the machine learning directory and install the project dependencies by running:

```sh
# Switch to the machine learning directory
cd ml

# Install machine learning dependencies
python -m pip install -r requirements-dev.txt
```

Additionally, the machine learning relies on a Databricks workspace and other Azure services. To set up your own instances of these services, use the deployment script:

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

### Interacting with Databricks

The deployment automatically sets up a Databricks workspace and stores connection information. As such, the Databricks CLI can be used to interface from the development environment with the workspace.

For example, this project uses [Databricks notebooks](https://docs.databricks.com/notebooks/index.html) as the entrypoint for triggered or scheduled data processing jobs. To synchronize notebook files from the local development environment to the Databricks workspace, use the following commands:

```sh
# Switch to the machine learning directory
cd ml

# Upload all local notebooks to the workspace
databricks workspace import_dir --overwrite --exclude-hidden-files ./jobs /Shared/ml/jobs
```

To share code between notebooks, this project uses a [Python wheel](https://pythonwheels.com/) as a [Databricks cluster library](https://docs.databricks.com/libraries/cluster-libraries.html). To upload a new version of the shared library to the Databricks workspace, use the following commands:

```sh
# Switch to the machine learning directory
cd ml

# Build the shared library
version="<change me>"
echo "$version" > version.txt
python ./setup.py bdist_wheel

# Upload the shared library to Databricks
databricks fs cp --overwrite "./dist/h3_utils-$version-py2.py3-none-any.whl" "dbfs:/libs/h3_utils-$version-py2.py3-none-any.whl"
```

### Code quality

#### Linting

This project utilizes [Flake8](https://flake8.pycqa.org/) to perform code linting and formatting. Most IDEs will recognize this and highlighting linting errors within the editor experience, however linting can be manually performed by running:

```sh
# Switch to the machine learning directory
cd ml

# Perform a lint check
flake8 .
```

### Deploying

Deployments occur automatically (via the configured [Azure DevOps Pipelines](https://dev.azure.com/healthnext/WBA-H3/_build)) when code changes are merged into the `main` branch. If you need to deploy a build manually however, you can use the API deployment script:

```sh
# Switch to the infrastructure directory
cd deployment

# Deploy only the machine learning code
pwsh ./deploy-ml.ps1 \
  -ResourceGroupName "<enter some app name here>" \
  -DeploymentName "resources"
```

## Links

- Project homepage: [https://dev.azure.com/healthnext/WBA-H3](https://dev.azure.com/healthnext/WBA-H3)
- Repository: [https://dev.azure.com/healthnext/\_git/WBA-H3](https://dev.azure.com/healthnext/_git/WBA-H3)
- Issue tracker: [https://dev.azure.com/healthnext/WBA-H3/\_workitems/](https://dev.azure.com/healthnext/WBA-H3/_workitems/)
- Build system: [https://dev.azure.com/healthnext/WBA-H3/\_build](https://dev.azure.com/healthnext/WBA-H3/_build)
