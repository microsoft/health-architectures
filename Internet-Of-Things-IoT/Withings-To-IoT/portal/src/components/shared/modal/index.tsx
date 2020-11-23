import React, { Fragment } from "react";
import styled from "styled-components";
import { ModalBaseStyles } from "./index.style";
import { Header } from "components/pages/shared/header";
import {} from "gatsby";

export const Modal = styled<
  React.FC<{
    className?: string;
    title?: string;
    HeaderComponent?: React.ComponentType;
    LeftPaneComponent?: React.ComponentType;
    RightPaneComponent?: React.ComponentType;
    Component?: React.ComponentType;
  }>
>(({ className, title, LeftPaneComponent, RightPaneComponent, HeaderComponent, Component }) => {
  return (
    <Fragment>
      <Header />
      <div className={`${className} modal`}>
        {(HeaderComponent && <HeaderComponent />) || (title && <header>{title}</header>)}
        <div className="modal-content">
          <Fragment>
            {LeftPaneComponent && <LeftPaneComponent />}
            {RightPaneComponent && <RightPaneComponent />}
            {Component && <Component />}
          </Fragment>
        </div>
      </div>
    </Fragment>
  );
})`
  ${ModalBaseStyles}
`;
