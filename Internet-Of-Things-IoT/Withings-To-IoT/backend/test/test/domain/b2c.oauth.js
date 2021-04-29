// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const timespan = require("timespan");
const config = require("../config");

class B2COAuth {
  login() {
    browser.url(
      `https://${config.B2C_TENANT_NAME}.b2clogin.com/${config.B2C_TENANT_NAME}.onmicrosoft.com/oauth2/v2.0/authorize` +
        `?p=${config.B2C_POLICY_NAME}` +
        `&client_id=${config.B2C_CLIENT_ID}` +
        `&nonce=defaultNonce` +
        `&redirect_uri=${encodeURIComponent(config.B2C_REDIRECT_URI)}` +
        `&scope=openid` +
        `&response_type=id_token` +
        `&prompt=login`
    );

    $("#logonIdentifier").setValue(config.B2C_USERNAME);
    $("#password").setValue(config.B2C_PASSWORD);
    $("#next").click();

    browser.waitUntil(() => browser.getUrl().startsWith(config.B2C_REDIRECT_URI), {
      timeout: timespan.fromSeconds(5).msecs,
      timeoutMsg: `Redirect to ${config.B2C_REDIRECT_URI} failed`,
    });

    return {
      idToken: browser.getUrl().match(/[#&]id_token=([A-Za-z0-9_.-]+)/)[1],
    };
  }
}

module.exports = new B2COAuth();
