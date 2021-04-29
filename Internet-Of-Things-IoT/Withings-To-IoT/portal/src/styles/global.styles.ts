import { createGlobalStyle } from "styled-components";
import tw from "twin.macro";

export const GlobalStyle = createGlobalStyle`
  html {
    background: #87c6e9;
    background: url("/gradient.svg") no-repeat center center fixed;
    -webkit-background-size: cover;
    -moz-background-size: cover;
    -o-background-size: cover;
    background-size: cover;
  }

  body {
    ${tw`font-sans-pro`}

    height: 100vh;
  }
`;
