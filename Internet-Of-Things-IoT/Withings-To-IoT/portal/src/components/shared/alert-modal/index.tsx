import media from "styled-media-query";
import React from "react";
import styled from "styled-components";
import { ModalBaseStyles } from "components/shared/modal/index.style";

export const AlertModal = styled<
  React.FC<{
    className?: string;
    title: string;
    Component: React.ComponentType;
  }>
>(({ className, title, Component }) => {
  return (
    <div className={`${className} modal single`}>
      <header>{title}</header>
      <div className="modal-content">
        <Component />
      </div>
    </div>
  );
})`
  &.modal.single {
    max-width: 460px;
    max-height: 447px;
    box-shadow: 4px 2px 6px rgba(72, 100, 125, 0.3), inset -2px -2px 6px rgba(72, 100, 125, 0.1);
    border-radius: 16px;
    ${media.lessThan("medium")`
      max-width: unset;
      max-height: unset;
    `}
  }
  ${ModalBaseStyles};
`;
