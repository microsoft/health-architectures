import { authProvider } from "auth/authProvider";
import { Paths } from "components/shared/Paths";
import { Link } from "gatsby";
import React from "react";
import styled from "styled-components";
import media from "styled-media-query";
import { theme } from "twin.macro";

export const Header = styled<React.FC<{ className?: string }>>(({ className }) => {
  return (
    <header className={className}>
      <div className="container">
        <div className="right-side">
          <h1>Horizon 3</h1>
          <nav className="nav-links">
            <ul>
              <li>
                <Link to={Paths.Dashboard} activeClassName="selected" partiallyActive={true}>
                  Health Monitoring
                </Link>
              </li>
              <li>
                <Link to={Paths.DeviceConnect} activeClassName="selected">
                  Manage Devices
                </Link>
              </li>
            </ul>
          </nav>
        </div>
        <div className="left-side">
          <button onClick={() => authProvider.logout()}>Logout</button>
        </div>
      </div>
    </header>
  );
})`
  & {
    color: ${theme`colors.white`};
    background-color: ${theme`colors.header.darkgray`};

    /* Little hack to get around the portal style (for now) */
    z-index: 100;

    position: absolute;
    width: 100%;

    ${media.lessThan("medium")`
      position: unset;
    `};

    .container {
      width: 100%;
      padding: 16px 32px;
      display: flex;
      align-content: center;
      justify-content: space-between;

      .right-side {
        display: flex;
        align-items: center;
      }

      .left-side {
        display: flex;
        align-items: center;
      }

      h1 {
        display: inline-block;
        margin-right: 50px;
        font-weight: bold;
        font-size: 1.5em;
        letter-spacing: -0.04em;
      }

      .nav-links {
        display: inline-block;

        ul {
          text-align: center;
        }

        li {
          display: inline;
        }

        a {
          display: inline-block;
          margin-right: 20px;
          font-weight: bold;
          font-size: 0.8em;
          color: ${theme`colors.header.lightgray`};

          &.selected {
            color: ${theme`colors.white`};
          }
        }

        ${media.lessThan("medium")`
        display: none;
      `}
      }
    }
  }
`;
