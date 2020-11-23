import "react-app-polyfill/ie11";
import "react-app-polyfill/stable";
import React from "react";
import NoSSR from "react-no-ssr";
import { AzureAD, AuthenticationState } from "react-aad-msal";
import { authProvider } from "auth/authProvider";
import { navigate } from "gatsby";
import { Paths } from "components/shared/Paths";

export default function Auth() {
  return (
    <NoSSR>
      <AzureAD provider={authProvider} forceLogin={true}>
        {({ authenticationState, error, accountInfo }: { authenticationState: any; error: any; accountInfo: any }) => {
          switch (authenticationState) {
            case AuthenticationState.Authenticated:
              console.log("account", accountInfo);
              navigate(Paths.DeviceConnect);
              return null;
            case AuthenticationState.Unauthenticated:
              return (
                <div>
                  {error && (
                    <p>
                      <span>An error occurred during authentication, please try again!</span>
                      <br />
                      <code>{error}</code>
                    </p>
                  )}
                </div>
              );
            case AuthenticationState.InProgress:
              return <p>Authenticating...</p>;
            default:
              return null;
          }
        }}
      </AzureAD>
    </NoSSR>
  );
}
