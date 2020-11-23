import { ModalBorderStyle } from "../modal/index.style";
import media from "styled-media-query";
import tw, { css, theme } from "twin.macro";

export const PanelItemStyles = css`
  cursor: pointer;
  width: 215px;
  height: 220px;
  border: ${ModalBorderStyle};
  border-radius: 12px;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 1rem 0;
  padding-bottom: 1rem;
  ${tw`flex-col`};

  img {
    flex: 1;
    object-fit: contain;
    max-height: 120px;
  }

  &.active {
    border: 3px solid ${theme`colors.blue`};
  }

  ${media.lessThan("large")`
    margin: 1rem auto;
  `}
`;
