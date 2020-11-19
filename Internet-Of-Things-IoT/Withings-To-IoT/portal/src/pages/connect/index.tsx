import get from "lodash/get";
import media from "styled-media-query";
import React, { Fragment } from "react";
import styled from "styled-components";
import tw from "twin.macro";
import { ButtonRound } from "components/shared/Button";
import { GetDeviceStatus, useGetDeviceStatus } from "components/pages/connect/hooks/useGetDeviceStatus";
import { Link } from "gatsby";
import { Merchant } from "components/pages/connect/MerchantRow";
import { MerchantRow } from "components/pages/connect/MerchantRow";
import { Modal } from "components/shared/modal";
import { Paths } from "components/shared/Paths";
import { PortalWithState } from "react-portal";
import { redirectToWithings } from "auth/withings";
import { StaticList, unusedMerchants, WithingsDevices, areDeviceTypesEqual } from "components/pages/connect/DeviceList";

type DeviceData = {
  name: string;
  className?: string;
  connectHandler?: () => void | null;
  isMerchantConnected: boolean;
  updateDevice: GetDeviceStatus["updateDevice"];
  devices: Merchant["devices"];
  logoImageUrl: string;
};

const StyledModal = styled(Modal)`
  &.merchant-device-info.modal {
    .modal-content {
      ${tw`flex py-2 px-1 w-full md:flex-col md:p-8 lg:p-8`}
    }
  }
`;

const CustomHeader = styled(({ className, isAuthed }) => {
  return (
    <header className={className} css={[tw`flex flex-col md:flex-row`]}>
      {isAuthed && (
        <Link to={Paths.Dashboard} css={[tw`flex items-center`]}>
          <ButtonRound css={[tw`text-small font-semibold`]}>Dashboard</ButtonRound>
        </Link>
      )}
      <span>{isAuthed ? "Device Manager" : "Connect Your First Account"}</span>
    </header>
  );
})`
  button {
    ${tw`absolute`};
    left: 2rem;
    width: 125px;
  }
  ${media.lessThan("medium")`
    height: 100px;
    line-height: 2rem;
    button {
      position: unset;
      order: 1;
    }
  `}
`;

const ConnectPage = () => {
  let DataToRender: Merchant[];
  let AllWithingsDevices: DeviceData | undefined = undefined;

  const hasWithingsCode = typeof window !== "undefined" && window.localStorage.getItem("withingsCode");
  const { deviceData, updateDevice, isUpdating, loadingDevices } = useGetDeviceStatus();

  if (deviceData.connectedDevices) {
    AllWithingsDevices = {
      name: "Withings",
      isMerchantConnected: true,
      connectHandler: redirectToWithings,
      logoImageUrl: "/withings-logo.png",
      updateDevice,
      devices: WithingsDevices.map(item => {
        const allDevices = [
          ...get(deviceData, "connectedDevices", []),
          ...get(deviceData, "disconnectedDevices", []),
        ];
        const foundDevice = allDevices.find(device => areDeviceTypesEqual(device.identifier, item.identifier));
        return {
          ...item,
          connected: deviceData.connectedDevices.some(device => areDeviceTypesEqual(device.identifier, item.identifier) && device.identifier),
          ...(foundDevice && { ...foundDevice }),
        };
      }),
    };
  }

  if (!hasWithingsCode || (hasWithingsCode && !AllWithingsDevices)) {
    DataToRender = StaticList;
  } else if (AllWithingsDevices) {
    DataToRender = [AllWithingsDevices, unusedMerchants];
  }

  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <StyledModal
              className="merchant-device-info"
              HeaderComponent={() => <CustomHeader isAuthed={hasWithingsCode && AllWithingsDevices} />}
              Component={() => (
                <Fragment>
                  {DataToRender.map((item, i) => {
                    return <MerchantRow {...item} isLoading={loadingDevices || isUpdating} key={i} />;
                  })}
                </Fragment>
              )}
            />
          )}
        </React.Fragment>
      )}
    </PortalWithState>
  );
};

export default ConnectPage;
