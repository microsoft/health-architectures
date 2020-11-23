import NoSSR from "react-no-ssr";
import React from "react";
import { authProvider } from "auth/authProvider";
import { Link } from "gatsby";

export default function Logout() {
  return (
    <NoSSR>
      <div>
        <h2>This is just a debug page to help with testing login</h2>
        <button onClick={() => authProvider.logout()}>Logout</button>
        <br />
        <Link to="/">Home</Link>
      </div>
    </NoSSR>
  );
}
