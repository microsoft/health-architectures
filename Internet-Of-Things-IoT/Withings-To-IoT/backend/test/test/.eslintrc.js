// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

module.exports = {
  env: {
    es6: true,
    node: true,
    mocha: true,
  },
  globals: {
    $: true,
    browser: true,
    expect: true,
  },
  plugins: [
    "mocha",
  ],
};
