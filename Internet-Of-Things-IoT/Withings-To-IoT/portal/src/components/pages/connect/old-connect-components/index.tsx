import media from "styled-media-query";
import React, { FC } from "react";
import styled from "styled-components";
import tw, { theme } from "twin.macro";
import { Modal } from "components/shared/modal";
import { ModalBorderStyle } from "components/shared/modal/index.style";

export const StyledDualPaneModal = styled(Modal)`
  & {
    .left-pane-container {
      ${tw`flex flex-1 justify-between flex-wrap`};
      padding: 4rem 3rem;
      border-right: ${ModalBorderStyle};
      ${(media.lessThan as any)("1320px")`
        justify-content: space-around;
      `}
    }

    .right-pane-container {
      width: 315px;
      ${media.lessThan("medium")`
        width: 100%;
        text-align: center;
      `}
    }
  }
`;

export const StyledCustomDescription = styled<
  FC<{
    className?: string;
    title: string;
    description: string;
  }>
>(({ className, title, description }) => {
  return (
    <div className={`${className} custom-description`}>
      <p className="title">{title}</p>
      <p className="description">{description}</p>
    </div>
  );
})`
  text-align: center;
  .title {
    padding: 0.5rem;
  }
  .description {
    ${tw`text-base`};
    color: ${theme`colors.gray`};
  }
`;
