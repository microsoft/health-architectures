import media from "styled-media-query";
import tw, { theme, css } from "twin.macro";

export const ModalBorderStyle = css`0.5px solid ${theme`colors.borders.gray`}`;

const HeaderStyles = css`
  header {
    ${tw`flex justify-center items-center font-semibold`}
    min-height: 72px;
    border-bottom: ${ModalBorderStyle};
    color: ${theme`colors.blue`};
  }
`;

export const ModalBaseStyles = css`
  &{
    &.modal {
      ${tw`flex flex-col text-large`};
      position: fixed;
      width: 80%;
      height: 80%;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      overflow: auto;
      background-color: ${theme`colors.white`};
      box-shadow: 4px 2px 6px rgba(72, 100, 125, 0.3), inset -2px -2px 6px rgba(72, 100, 125, 0.1);
      border-radius: 10px;
      margin-top: 2rem;
      ${media.lessThan("medium")`
        width: 100%;
        height: 100%;
        margin-top: 68px;
        border-radius: 0;
      `}
    }

    ${HeaderStyles};

    .modal-content {
      ${tw`flex flex-1`}
      background-color: #fefefe;
      ${media.lessThan("medium")`
        flex-direction: column;
      `}
  }
`;
