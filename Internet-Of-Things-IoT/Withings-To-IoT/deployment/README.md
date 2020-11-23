# Withings IoMT deployment

Deployment scripts and Infrastructure-as-Code for the Withings IoMT platform.

## Table of Contents

- [Features](#features)
- [Built with](#built-with)
- [Getting started](#getting-started)
  - [Clone the mono-repo](#clone-the-mono-repo)
  - [Install project dependencies](#install-project-dependencies)
  - [Run the scripts](#run-the-scripts)
  - [Code quality](#code-quality)
    - [Linting](#linting)
- [Links](#links)

## Features

- Fully automated, hands-off setup of infrastructure
- Incremental deployments

## Built with

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/what-is-azure-cli)
- [Azure Resource Manager Templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview)

## Getting started

To clone and run this application, you will need [Git](https://git-scm.com/), [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell), [DotNet Core](https://docs.microsoft.com/en-us/dotnet/core/install/), [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli), [Python](https://www.python.org/downloads/), [Databricks CLI](https://docs.microsoft.com/en-us/azure/databricks/dev-tools/cli/) and [Node.js](https://nodejs.org/) (which comes with [npm](https://www.npmjs.com/)) installed on your computer. Opening the project in [VSCode](https://code.visualstudio.com/) using the provided [devcontainer](https://code.visualstudio.com/docs/remote/containers) will automatically set up an environment that contains all the required tools.

### Clone the mono-repo

To get started you will need to clone the H3 mono-repo to your machine. You can do this from your terminal by running:

```sh
# Clone the mono-repo
$ git clone https://healthnext@dev.azure.com/healthnext/WBA-H3/_git/WBA-H3
```

Alternately, you can use the [tooling provide in Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio) to clone the repository using your preferred IDE.

### Install project dependencies

After cloning the mono-repo, switch to the deployment directory and install the project dependencies by running:

```sh
# Install deployment dependencies
pwsh deployment/install-dependencies.ps1
```

### Run the scripts

The following scripts are available:

| Name | Purpose |
|------|---------|
| `deploy.ps1` | Deploy all Azure resources required for the H3 platform |
| `cleanup.ps1` | Delete H3 platform deployments |
| `seed.ps1` | Populate a FHIR server with synthetic data |

More information about each script can be found in comments at the top of the file.

### Code quality

#### Linting

This project utilizes [PowerShell Script Analyzer](https://github.com/PowerShell/PSScriptAnalyzer) to perform code linting. To perform linting, run the following:

```sh
# Switch to the deployment directory
cd deployment

# Perform a lint check
pwsh -Command 'Invoke-ScriptAnalyzer -EnableExit -Path *.ps*1'
```
