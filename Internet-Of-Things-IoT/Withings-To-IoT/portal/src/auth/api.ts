export enum ApiPaths {
  User = "/user",
  Observations = "/observations",
  WithingsAuth = "/withings/auth"
}

// helper function to access the resource with the token
function callApiWithAccessToken(
  method: string,
  endpoint: string,
  token: string,
  data: object | null = null
): Promise<Response> {
  const headers = new Headers();
  const bearer = `Bearer ${token}`;

  headers.append("Authorization", bearer);

  const url = getBasePath() + endpoint;

  const options = {
    method: method,
    headers: headers,
    body: "",
  };

  if (data) {
    options.body = JSON.stringify(data);
    options.headers.append("Accept", "application/json");
    options.headers.append("Content-Type", "application/json");
  }

  return fetch(url, options)
    .then(response => response.json())
    .catch(error => {
      console.log("Error calling the Web api:\n" + error);
    });
}

export function setUserWithingsCode(token: string, withingsCode: string): Promise<Response> {
  const data = {
    withingsAccessCode: withingsCode,
    withingsRedirectUri: process.env.GATSBY_WITHINGS_REDIRECT_URL,
  };

  return callApiWithAccessToken("POST", ApiPaths.WithingsAuth, token, data);
}


export function getAuthHeaders() {
  return {
    Accept: "application/json",
    "Content-Type": "application/json",
    authorization: `Bearer ${typeof window !== "undefined" && window.localStorage.getItem("msalAuthToken")}`,
  };
}

// There are some CORS restrictions with the API, so this takes advantage of the proxy used from gatsby.
export function getBasePath() {
  return process.env.NODE_ENV === "development" ? "/api" : process.env.GATSBY_H3_API_BASE_PATH;
}

export function removeLocalStorageItems(arr: ("msalAuthToken" | "withingsCode")[]) {
  arr.forEach(item => localStorage.removeItem(item));
}
