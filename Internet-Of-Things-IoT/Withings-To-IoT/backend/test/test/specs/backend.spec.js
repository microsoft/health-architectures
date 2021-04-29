// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

const should = require("should");
const timespan = require("timespan");
const B2C = require("../domain/b2c.oauth");
const Withings = require("../domain/withings.oauth");
const Api = require("../domain/h3.api");

function isConflict(ex) {
  return ex.message === "Conflict";
}

describe("A user of the H3 backend", () => {
  let b2cAuth, withingsAuth, api, user;

  it("should authenticate with Azure B2C", () => {
    b2cAuth = B2C.login();

    should(b2cAuth).be.instanceof(Object);
    should(b2cAuth).have.property("idToken");
    should(b2cAuth.idToken).be.instanceof(String);
  });

  it("should authenticate with Withings", () => {
    withingsAuth = Withings.login();

    should(withingsAuth).be.instanceof(Object);
    should(withingsAuth).have.property("withingsAccessCode");
    should(withingsAuth).have.property("withingsRedirectUri");
    should(withingsAuth.withingsAccessCode).be.instanceof(String);
    should(withingsAuth.withingsRedirectUri).be.instanceof(String);
  });

  it("should authenticate with the API", () => {
    api = new Api(b2cAuth);
    user = api.withingsAuth(withingsAuth);

    should(user).be.instanceof(Object);
    should(user).have.property("disconnectedDevices");
    should(user).have.property("connectedDevices");
    should(user.disconnectedDevices).be.instanceof(Array);
    should(user.connectedDevices).be.instanceof(Array);
    should(user.connectedDevices).have.length(0);
    should(user.disconnectedDevices).have.length(2);
  });

  it("should connect devices via the API", () => {
    const deviceIdToConnect = user.disconnectedDevices[0].identifier;

    user = api.createOrUpdateUser({
      connectedDeviceIds: [deviceIdToConnect],
    });

    should(user.connectedDevices).have.length(1);
    should(user.disconnectedDevices).have.length(1);
    should(user.jobId).not.be.null();

    should.throws(
      () => api.createOrUpdateUser({ connectedDeviceIds: [deviceIdToConnect] }),
      isConflict,
      "Connecting devices should block other operations"
    );

    api.waitForJob(user.jobId, {
      timeout: timespan.fromMinutes(5).msecs,
      timeoutMsg: `Job to connect device ${deviceIdToConnect.value} didn't complete`,
    });

    const observations = api.getObservations();

    should(observations).not.have.length(0);
  });

  it("should disconnect devices via the API", () => {
    const deviceIdToDisconnect = user.connectedDevices[0].identifier;

    user = api.createOrUpdateUser({
      disconnectedDeviceIds: [deviceIdToDisconnect],
    });

    should(user.jobId).not.be.null();

    should.throws(
      () => api.createOrUpdateUser({ disconnectedDeviceIds: [deviceIdToDisconnect] }),
      isConflict,
      "Disconnecting devices should block other operations"
    );

    api.waitForJob(user.jobId, {
      timeout: timespan.fromMinutes(5).msecs,
      timeoutMsg: `Job to disconnect device ${deviceIdToDisconnect.value} didn't complete`,
    });

    const observations = api.getObservations();

    should(observations).have.length(0);
  });

  after(() => {
    if (api == null) {
      return;
    }

    const { jobId } = api.deleteUser();

    api.waitForJob(jobId, {
      timeout: timespan.fromMinutes(5).msecs,
      timeoutMsg: `Job to delete user didn't complete`,
    });
  });
});
