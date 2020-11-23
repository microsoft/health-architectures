import styled from "styled-components";
import tw, { theme } from "twin.macro";

export const Button = styled.button.attrs<{ className?: string }>({
  className: "h3-button",
})`
  ${tw`outline-none`};
  width: 250px;
  height: 35px;
  color: white;
  background: linear-gradient(90deg, #6a81e4 0%, #5966e2 100%);
  border: none;
  border-radius: 8px;
  &:active {
    transform: translate(0, 1px);
  }
  &.invert {
    color: ${theme`colors.gray`};
    background: white;
    border: 1px solid ${theme`colors.gray`};
  }

  &:hover {
    color: ${theme`colors.white`};
    background: ${theme`colors.psuedo.hover`};
  }
`;

export const ButtonRound = styled(Button)`
  ${tw`rounded-full`}
`;
