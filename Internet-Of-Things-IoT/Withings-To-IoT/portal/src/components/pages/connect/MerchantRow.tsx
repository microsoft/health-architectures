import media from "styled-media-query";
import React from "react";
import styled from "styled-components";
import tw, { theme } from "twin.macro";
import { Button } from "components/shared/Button";
import { DeviceInfo, GetDeviceStatus } from "./hooks/useGetDeviceStatus";
import { Link } from "gatsby";
import { ModalBorderStyle } from "components/shared/modal/index.style";
import { Paths } from "components/shared/Paths";

export interface Merchant {
  className?: string;
  name: string;
  isLoading?: boolean;
  isMerchantConnected: boolean;
  logoImageUrl: string;
  updateDevice?: GetDeviceStatus["updateDevice"];
  connectHandler?: any;
  devices: Partial<
    {
      name: string;
      imageUrl: string;
      type: string;
      connected: boolean;
    } & DeviceInfo
  >[];
}

const MerchantRowStyles = (Component: React.FC<Merchant>) => styled(Component)`
  &.merchant-info {
    height: 160px;
    border: ${ModalBorderStyle};
    border-radius: 12px;
    ${media.lessThan("medium")`
      height: unset;
    `}
  }
  .vendor {
    min-width: 240px;
    img {
      height: 50px;
      width: 150px;
    }
  }
  .tile {
    min-width: 200px;
    border-left: ${ModalBorderStyle};
    &:last-child {
      border-right: ${ModalBorderStyle};
    }
    &:hover {
      opacity: 0.7;
    }
    img {
      margin: 0 auto;
    }
    .text {
      color: ${theme`colors.gray1`};
    }
    ${media.lessThan("medium")`
      &:first-child {
        border-left: none;
      }
    `}
  }
  .tile .connected-status {
    display: flex;
    align-items: center;
    margin: 0 auto;
    .connected-icon {
      height: 10px;
      width: 10px;
      background: green;
      border-radius: 10px;
      margin-right: 8px;
    }
  }
  button {
    ${tw`text-small font-semibold outline-none`};
    width: 175px;
  }
`;

export const MerchantRow = MerchantRowStyles(({ className, ...props }) => {
  return (
    <div
      className={`${className} merchant-info`}
      css={[
        tw`flex flex-col mb-4 w-full md:flex-row cursor-pointer`,
        props.isLoading && tw`opacity-50 pointer-events-none`,
      ]}
    >
      <div className="vendor" css={[tw`flex flex-col justify-around items-center py-4`]}>
        <img src={props.logoImageUrl} alt="" />
        {(props.isMerchantConnected && (
          <Link to={Paths.Dashboard}>
            <Button className="invert">More</Button>
          </Link>
        )) || <Button onClick={props.connectHandler}>Connnect to {props.name}</Button>}
      </div>
      <div className="responsive-wrap" css={[tw`flex w-full overflow-x-auto`]}>
        {props.devices.map((item, i) => {
          return (
            <div
              key={i}
              className="tile"
              css={[tw`flex flex-col justify-around active:opacity-75`, !item.identifier && tw`pointer-events-none`]}
              onClick={() => {
                if (item.identifier) {
                  props.updateDevice?.({ identifier: item.identifier });
                }
              }}
            >
              <img src={item.imageUrl} alt="" css={[tw`w-24`]} />
              <div className="connected-status" css={[tw`text-small text-center`]}>
                {item.identifier && item.connected && <div className="connected-icon" />}
                <span>{item.identifier && item.connected ? "Connected" : "Disconnected"}</span>
              </div>
              <p className="text" css={[tw`text-small text-center font-semibold`]}>
                {item.name}
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
});
