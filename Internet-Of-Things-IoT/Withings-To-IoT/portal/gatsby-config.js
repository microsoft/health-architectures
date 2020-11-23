// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

require("dotenv").config({
  path: ".env",
});

module.exports = {
  proxy: {
    prefix: "/api",
    url: "https://h3devapi.azure-api.net",
  },
  siteMetadata: {
    title: "H3 portal",
    description: "Web application to access the H3 system",
    author: "@microsoft",
  },
  plugins: [
    {
      resolve: "gatsby-plugin-alias-imports",
      options: {
        alias: {
          components: "src/components",
          content: "src/content",
          auth: "src/auth",
        },
        extensions: ["js", "tsx", "ts"],
      },
    },
    "gatsby-plugin-react-helmet",
    "gatsby-plugin-styled-components",
    "gatsby-plugin-postcss",
    {
      resolve: "gatsby-plugin-purgecss",
      options: {
        tailwind: true,
        purgeOnly: ["src/css/index.css"],
      },
    },
    "gatsby-plugin-typescript",
    "gatsby-plugin-typescript-checker",
  ],
};
