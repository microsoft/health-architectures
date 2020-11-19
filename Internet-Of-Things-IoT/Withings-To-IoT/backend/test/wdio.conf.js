// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const timespan = require("timespan");

exports.config = {
  host: "localhost",
  specs: ["./test/specs/**/*.js"],
  capabilities: [
    {
      browserName: "chrome",
      "goog:chromeOptions": {
        args: [
          "--headless",
          "--disable-gpu",
        ],
      },
    },
  ],
  sync: true,
  logLevel: "debug",
  bail: 1,
  waitforTimeout: timespan.fromSeconds(10).msecs,
  connectionRetryTimeout: timespan.fromMinutes(2).msecs,
  connectionRetryCount: 3,
  services: ["docker"],
  dockerLogs: "./",
  dockerOptions: {
    image: "selenium/standalone-chrome",
    healthCheck: "http://localhost:4444",
    options: {
      p: ["4444:4444"],
      shmSize: "2g",
    },
  },
  framework: "mocha",
  mochaOpts: {
    ui: "bdd",
    timeout: timespan.fromMinutes(30).msecs,
  },
  reporters: ["spec"],
};
