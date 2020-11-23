// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

exports.onCreateWebpackConfig = ({ stage, loaders, actions }) => {
  if (stage === "build-html") {
    actions.setWebpackConfig({
      module: {
        rules: [
          {
            test: /authProvider|AzureAD/,
            use: loaders.null(),
          },
        ],
      },
    })
  }
}
