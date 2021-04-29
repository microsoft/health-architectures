// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const fetch = require("node-fetch");
const config = require("../config");

function callApi(endpoint, request) {
  const url = `${config.API_ENDPOINT}${endpoint}`;

  return browser.call(() =>
    fetch(url, request).then(response => {
      console.log(`[${new Date()}] ${request.method} ${url} => ${response.status}`);

      if (!response.ok) {
        throw new Error(response.statusText);
      }

      return response.json();
    })
  );
}

class Api {
  constructor({ idToken }) {
    this.idToken = idToken;
  }

  getObservations() {
    return callApi("/observations", {
      method: "GET",
      headers: {
        Authorization: `Bearer ${this.idToken}`,
      },
    });
  }

  createOrUpdateUser(payload) {
    return callApi("/user", {
      method: "POST",
      body: JSON.stringify(payload),
      headers: {
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      },
    });
  }

  deleteUser() {
    return callApi("/user", {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${this.idToken}`,
      },
    });
  }

  getStatus(jobId) {
    return callApi(`/jobs?jobId=${jobId}`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${this.idToken}`,
      },
    });
  }

  withingsAuth(payload) {
    return callApi("/withings/auth", {
      method: "POST",
      body: JSON.stringify(payload),
      headers: {
        Authorization: `Bearer ${this.idToken}`,
        "Content-Type": "application/json",
      },
    });
  }

  waitForJob(jobId, { timeout, timeoutMsg }) {
    let status;

    browser.waitUntil(
      () => {
        status = this.getStatus(jobId).status;
        return status === "Completed" || status === "Failed";
      },
      {
        timeout,
        timeoutMsg,
      }
    );

    if (status === "Failed") {
      throw new Error(`Job ${jobId} failed`);
    }
  }
}

module.exports = Api;
