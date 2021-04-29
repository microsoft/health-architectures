// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const tailwindcss = require("tailwindcss")

module.exports = {
  plugins: [
    tailwindcss("./tailwind.config.js"),
    require("autoprefixer"),
    require("cssnano")({
      preset: "default",
    }),
  ],
}
