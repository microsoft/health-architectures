import React from "react";
import styled from "styled-components";
import { HalfPaneStyles } from "./HalfPane";
import tw, { theme, css } from "twin.macro";

export const HealthPane = HalfPaneStyles<{
  headerTitle?: string;
}>(
  ({ className, children, headerTitle }) => {
    return (
      <div className={className}>
        <header className="title-header">{headerTitle}</header>
        {children}
      </div>
    );
  },
  css`
    & {
      ${tw`flex flex-col items-center md:items-start`};
      header.title-header {
        height: 50px;
        border: unset;
        justify-content: unset;
        ${theme`colors.blue`};
      }
      a:last-child .devices {
        line-height: 3rem;
        img {
          margin-top: 2rem;
        }
      }
    }
  `
);

export const Tile = styled.div<{
  width?: string;
  height?: string;
}>`
  ${tw`flex flex-col font-bold`};
  border: 1px solid ${theme`colors.gray`};
  height: ${props => `${props.height}px`};
  width: ${props => `${props.width}px`};
  border-radius: 12px;
  &:hover,
  &:active {
    border: 2px solid ${theme`colors.blue`};
    border-radius: 17px;
    cursor: pointer;
  }
  .info {
    .type {
      color: ${theme`colors.darkBlack`};
      ${tw`text-base`};
    }
    .timestamp {
      color: ${theme`colors.gray1`};
      ${tw`font-semibold`};
      font-size: 13px;
    }
  }
  .measurement {
    color: ${theme`colors.blue`};
    ${tw`flex-1 font-semibold`};
    font-size: 44px;
  }
  .unit {
    color: ${theme`colors.blue`};
    font-size: 24px;
  }
`;

export const ImageTile = styled(Tile)`
  img {
    margin: 0 auto;
  }
  .brand {
  }
  .description {
    color: ${theme`colors.gray1`};
  }
`;
