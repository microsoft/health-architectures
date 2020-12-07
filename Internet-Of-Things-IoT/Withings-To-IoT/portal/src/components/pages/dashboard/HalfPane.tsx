import React, { FC } from "react";
import styled, { FlattenSimpleInterpolation } from "styled-components";
import media from "styled-media-query";

interface HalfPaneProps {
  className?: string;
  children?: React.ReactChild;
}

export const HalfPaneStyles = <T extends {}>(
  Component: FC<T & HalfPaneProps>,
  styles?: FlattenSimpleInterpolation
) => styled(Component)`
  height: 50%;
  ${media.lessThan("medium")`
    height: unset;
  `}
  ${styles && styles};
`;
