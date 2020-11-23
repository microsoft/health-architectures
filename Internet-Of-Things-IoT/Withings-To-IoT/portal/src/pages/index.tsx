import NoSSR from "react-no-ssr";
import React, { ReactElement } from "react";
import { AuthenticationState, AzureAD } from "react-aad-msal";
import { authProvider } from "auth/authProvider";
import { navigate } from "gatsby";
import { Paths } from "components/shared/Paths";

function Index(): ReactElement {
  return (
    <NoSSR>
      <AzureAD provider={authProvider} forceLogin={true}>
        {({ authenticationState, accountInfo }: { authenticationState: any; accountInfo: any }) => {
          switch (authenticationState) {
            case AuthenticationState.Authenticated:
              console.log("account", accountInfo);
              navigate(Paths.DeviceConnect);
              return null;
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

export default Index;
