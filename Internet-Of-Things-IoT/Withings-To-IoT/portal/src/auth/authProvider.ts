import { Configuration } from "msal";
import { MsalAuthProvider, LoginType } from "react-aad-msal";

const b2cTenantName = `${process.env.GATSBY_B2C_TENANT_NAME}`;
const b2cPolicySUSI = `${process.env.GATSBY_B2C_POLICY_SIGNUP_SIGNIN}`
const b2cPolicyForgotPassword = `${process.env.GATSBY_B2C_POLICY_FORGOT_PASSWORD}`

const authorityPrefix: string = `https://${b2cTenantName}.b2clogin.com/${b2cTenantName}.onmicrosoft.com`;

const b2cPolicies = {
  names: {
    signUpSignIn: b2cPolicySUSI,
    forgotPassword: b2cPolicyForgotPassword,
  },
  authorities: {
    signUpSignIn: {
      authority: `${authorityPrefix}/${b2cPolicySUSI}`,
    },
    forgotPassword: {
      authority: `${authorityPrefix}/${b2cPolicyForgotPassword}`,
    },
  },
};

// Msal Configurations
const config: Configuration = {
  auth: {
    authority: b2cPolicies.authorities.signUpSignIn.authority,
    clientId: process.env.GATSBY_B2C_APP_ID!,
    redirectUri: process.env.GATSBY_B2C_REDIRECT,
    validateAuthority: false,
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: true,
  },
};

// Authentication Parameters
const authenticationParameters = {
  scopes: ["openid", "profile"],
};

// Options
const options = {
  loginType: LoginType.Redirect,
  tokenRefreshUri: process.env.GATSBY_B2C_REDIRECT,
};

export const authProvider = new MsalAuthProvider(config, authenticationParameters, options);
