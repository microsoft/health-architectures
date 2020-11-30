# Withings Web Client Portal

Gatsby based client portal for H3.

## Table of Contents

- [Features](#features)
- [Built with](#built-with)
- [Getting started](#getting-started)
  - [Clone the mono-repo](#clone-the-mono-repo)
  - [Install project dependencies](#install-project-dependencies)
  - [Start the development server](#start-the-development-server)
  - [Code quality](#code-quality)
    - [Type checking](#type-checking)
    - [Linting](#linting)
  - [Building](#building)
  - [Deploying](#deploying)
- [Links](#links)

## Features

- Identity provided by [Azure Active Directory B2C](https://azure.microsoft.com/en-us/services/active-directory/external-identities/b2c/)
- Fully responsive UX
- Secure interaction with IoMT devices

## Built with

- [Azure](https://azure.microsoft.com/) - Microsoft's cloud computing platform.
- [GatsbyJS](https://www.gatsbyjs.org/) - Static site generator web framework.
- [TypeScript](https://www.typescriptlang.org/) - Typed superset of JavaScript.
- [TailwindCSS](https://tailwindcss.com/) - A utility-first CSS framework.

## Getting started

To clone and run this application, you will need [Git](https://git-scm.com/) and [Node.js](https://nodejs.org/) (which comes with [npm](https://www.npmjs.com/)) installed on your computer. Opening the project in [VSCode](https://code.visualstudio.com/) using the provided [devcontainer](https://code.visualstudio.com/docs/remote/containers) will automatically set up an environment that contains all the required tools.

### Clone the mono-repo

To get started you will need to clone the H3 mono-repo to your machine. You can do this from your terminal by running:

```sh
# Clone the mono-repo
$ git clone https://healthnext@dev.azure.com/healthnext/WBA-H3/_git/WBA-H3
```

Alternately, you can use the [tooling provide in Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio) to clone the repository using your preferred IDE.

### Install project dependencies

After cloning the mono-repo, switch to the portal app directory and install the project dependencies by running:

```sh
# Switch to the portal app directory
$ cd portal

# Install dependencies
$ npm install
```

### Replace environment variables

Environment variables are used to configure the needed dependencies. The file is located in `env/.env.development`

- Azure AD B2C account (includes B2C policies for forgotten passwords, sign in & sign up)

- Withings account

- Base endpoint path for api calls

### Start the development server

To start the local development server, from your terminal run:

```sh
# Start the local development server
$ npm start
```

This will start the application in [develop mode](https://www.gatsbyjs.org/docs/quick-start/#start-development-server). Once the server has started, navigate to [https://localhost:8000/](https://localhost:8000/) to see the application running. From here, any code changes will be hot-reloaded and appear in browser without having to restart the server.

### Code quality

#### Type Checking

This project utilizes [TypeScript](https://www.typescriptlang.org/) for ensuring type safety. Most IDEs will recognize this and highlight type issues within the editor experience, however types can be manually checked by running:

```sh
# Perform a type check
$ npm run type-check
```

#### Linting

This project utilizes [ESLint](https://eslint.org/) and [Prettier](https://prettier.io/) to perform code linting and formatting. Most IDEs will recognize this and highlighting linting errors within the editor experience, however linting can be manually performed by running:

```sh
# Perform a lint check
$ npm run eslint
```

### Building

In order to conduct a build for deployment, run the following in your terminal:

```sh
# Conduct a build
$ npm run build
```

The build script will build the H3 client portal app for deployment and create a self-contained set of HTML/JS/CSS files ready to be deployed to a static web host such as [Azure Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website). The output of the build can be found in the `public` directory of the portal app.

The build will also render some [nunjucks templates](https://mozilla.github.io/nunjucks/) to generate HTML content which is used [customize the Active Directory B2C login flow](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-ui-customization) and output them into the same `public` folder.

### Deploying

Deployments occur automatically (via the configured [Azure DevOps Pipelines](https://dev.azure.com/healthnext/WBA-H3/_build)) when code changes are merged into the `main` branch. If you need to deploy a build manually however, take the output from the [build](#building) step above and upload it to the project's storage container in Azure. This can be done via the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/storage/blob?view=azure-cli-latest#az-storage-blob-upload), or with various extensions/tooling within your IDE (such as the [Azure Storage extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurestorage) for Visual Studio Code)

## Links

- Project homepage: [https://dev.azure.com/healthnext/WBA-H3](https://dev.azure.com/healthnext/WBA-H3)
- Repository: [https://dev.azure.com/healthnext/\_git/WBA-H3](https://dev.azure.com/healthnext/_git/WBA-H3)
- Issue tracker: [https://dev.azure.com/healthnext/WBA-H3/\_workitems/](https://dev.azure.com/healthnext/WBA-H3/_workitems/)
- Build system: [https://dev.azure.com/healthnext/WBA-H3/\_build](https://dev.azure.com/healthnext/WBA-H3/_build)
