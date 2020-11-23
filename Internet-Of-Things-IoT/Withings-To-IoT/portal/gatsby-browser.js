// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import "tailwindcss/dist/base.min.css";
import "react-app-polyfill/ie11";
import "react-app-polyfill/stable";
import React, { Fragment } from "react";
import { Helmet } from "react-helmet";
import { GlobalStyle } from "./src/styles/global.styles";
import { authProvider } from "auth/authProvider";
import { AuthenticationState } from "react-aad-msal";

const wrapRootElement = ({ element }) => {
  return (
    <Fragment>
      <Helmet>
        <link
          href="https://fonts.googleapis.com/css2?family=Source+Sans+Pro:wght@400;700&display=swap"
          rel="stylesheet"
          preload="true"
        />
      </Helmet>
      <GlobalStyle />
      {element}
    </Fragment>
  );
};

const wrapPageElement = ({element, props}) => {

  if (typeof window !== "undefined" && authProvider.authenticationState === AuthenticationState.Unauthenticated) {

    const error = authProvider.getError();

    if (error) {
      console.error(error);

      return (
        <>
          <h3>Authentication error:</h3>
          <div>{error.errorMessage}</div>
        </>
      )
    } else {
      authProvider.login();
    }

  }

  return (
    <>
      {element}
    </>
  )
}

export { wrapRootElement, wrapPageElement };
