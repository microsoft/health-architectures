// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const path = require("path");
const envalid = require("envalid");

module.exports = envalid.cleanEnv(
  process.env,
  {
    B2C_TENANT_NAME: envalid.str(),
    B2C_CLIENT_ID: envalid.str(),
    B2C_POLICY_NAME: envalid.str(),
    B2C_REDIRECT_URI: envalid.url(),
    B2C_USERNAME: envalid.str(),
    B2C_PASSWORD: envalid.str(),

    WITHINGS_CLIENT_ID: envalid.str(),
    WITHINGS_REDIRECT_URI: envalid.url(),
    WITHINGS_USERNAME: envalid.str(),
    WITHINGS_USER_ID: envalid.str(),
    WITHINGS_PASSWORD: envalid.str(),

    API_ENDPOINT: envalid.url(),
  },
  {
    dotEnvPath: path.join(__dirname, "..", ".env"),
  }
);
