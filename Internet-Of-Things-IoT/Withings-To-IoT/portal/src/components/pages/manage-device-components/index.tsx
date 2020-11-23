import media from "styled-media-query"
import styled from "styled-components"
import { TogglePanel } from "components/shared/TogglePanel"
import tw from "twin.macro"

export const StyledTogglePanel = styled(TogglePanel)`
  width: 100%;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-evenly;
  padding: 2rem;

  ${media.lessThan("medium")`
    flex-direction: column;
    align-items: center;
    align-content: center;
  `}

  .panel-item {
    width: 160px;
    height: 160px;
    margin: 0.5rem;
    img {
      max-height: 105px;
    }
  }
  .button-group {
    display: flex;
    flex-direction: column;
    align-self: flex-end;
    margin-top: 1rem;
    button {
      margin: 0.5rem 0;
    }
  }
  .connected-text {
    ${tw`text-xs`};
  }
  .connected-image {
    position: relative;
    display: flex;
    flex-direction: column;
    visibility: hidden;
  }
  .connected-image.show {
    visibility: visible;
  }
`
