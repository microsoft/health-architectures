import { navigate } from "gatsby";
import cryptoRandomString from "crypto-random-string";

export function redirectToWithings() {
  const withingsAuthorizeUri = "https://account.withings.com/oauth2_user/authorize2";
  const withingsState = cryptoRandomString({ length: 10 });

  localStorage.setItem("withings_state", withingsState);

  const queryParams = {
    response_type: "code",
    client_id: process.env.GATSBY_WITHINGS_CLIENT_ID,
    state: withingsState,
    scope: "user.info,user.metrics",
    redirect_uri: process.env.GATSBY_WITHINGS_REDIRECT_URL,
    b: "authorize2",
  };

  const queryString = Object.entries(queryParams)
    .map(([key, value]) => `${key}=${encodeURIComponent(value!)}`)
    .join("&");

  const url = `${withingsAuthorizeUri}?${queryString}`;

  navigate(url);
}
