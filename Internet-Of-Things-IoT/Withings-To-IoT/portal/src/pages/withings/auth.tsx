import React, { useEffect } from "react";
import { navigate } from "gatsby";
import { setUserWithingsCode } from "auth/api";
import { authProvider } from "auth/authProvider";
import { Paths } from "components/shared/Paths";

export default function Auth() {
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get("code");
    const state = urlParams.get("state");
    const error = urlParams.get("error");

    const withingsState = localStorage.getItem("withings_state");
    if (state?.toString() !== withingsState?.toString()) {
      console.error("Nonce does not match returned state.", state, withingsState);
      navigate(Paths.DeviceConnect);
    }

    if (code) {
      authProvider.getIdToken().then(token => {
        const idToken = token.idToken.rawIdToken;
        localStorage.setItem("withingsCode", String(code));
        localStorage.setItem("msalAuthToken", token.idToken.rawIdToken);

        setUserWithingsCode(idToken, code!).finally(() => {
          navigate(Paths.DeviceConnect);
        });
      });
    } else {

      // Access denied is usually when the user purposefully clicks "refuse" during Withings auth
      if (error && error !== "access_denied") {
        console.error(`Withings error: ${error}`);
      }

      navigate(Paths.DeviceConnect);
    }
  }, []);

  return <div>Registering Withings account...</div>;
}
