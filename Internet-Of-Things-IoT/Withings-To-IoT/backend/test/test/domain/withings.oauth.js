// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const timespan = require("timespan");
const config = require("../config");

class WithingsOAuth {
  login() {
    const scopes = ["user.metrics", "user.info"];

    const state = Math.random()
      .toString()
      .substring(2);

    browser.url(
      "https://account.withings.com/oauth2_user/account_login" +
        "?response_type=code" +
        `&client_id=${config.WITHINGS_CLIENT_ID}` +
        `&state=${state}` +
        `&scope=${encodeURIComponent(scopes.join(","))}` +
        `&redirect_uri=${encodeURIComponent(config.WITHINGS_REDIRECT_URI)}` +
        `&selecteduser=${config.WITHINGS_USER_ID}`
    );

    $('[name="email"]').setValue(config.WITHINGS_USERNAME);
    $('[name="password"]').setValue(config.WITHINGS_PASSWORD);
    $('[type="submit"]').click();

    browser.waitUntil(() => browser.getUrl().startsWith("https://account.withings.com/oauth2_user/authorize2"), {
      timeout: timespan.fromSeconds(5).msecs,
      timeoutMsg: `Redirect to Withings authorization page failed`,
    });

    $('[name="authorized"]').click();

    browser.waitUntil(() => browser.getUrl().startsWith(config.WITHINGS_REDIRECT_URI), {
      timeout: timespan.fromSeconds(5).msecs,
      timeoutMsg: `Redirect to ${config.WITHINGS_REDIRECT_URI} failed`,
    });

    return {
      withingsAccessCode: browser.getUrl().match(/[?&]code=([a-zA-Z0-9]+)/)[1],
      withingsRedirectUri: config.WITHINGS_REDIRECT_URI,
    };
  }
}

module.exports = new WithingsOAuth();
