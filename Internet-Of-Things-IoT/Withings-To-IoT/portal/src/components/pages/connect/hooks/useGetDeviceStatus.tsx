import useFetch, { CachePolicies } from "use-http";
import { getAuthHeaders, ApiPaths, getBasePath } from "auth/api";
import { useEffect, useCallback } from "react";
import { areDeviceTypesEqual, DeviceIdentifier } from "components/pages/connect/DeviceList"

export type DeviceInfo = {
  battery: string;
  lastSessionDate: number;
  model: string;
  timezone: string;
  identifier: DeviceIdentifier
};
export interface GetDeviceStatus {
  deviceData: {
    connectedDevices: DeviceInfo[];
    disconnectedDevices: DeviceInfo[];
  };
  loadingDevices: boolean;
  isUpdating: boolean;
  loadingDevicesError: Error;
  updateDevice: ({ identifier }: { identifier: DeviceIdentifier }) => void;
}

export const useGetDeviceStatus = (): GetDeviceStatus => {
  const { get, loading: loadingDevices, error: loadingDevicesError, data: deviceData = [] } = useFetch(getBasePath(), {
    headers: getAuthHeaders(),
    cachePolicy: CachePolicies.NO_CACHE,
  });

  const { post, loading: isUpdating } = useFetch(getBasePath(), {
    headers: getAuthHeaders(),
    cachePolicy: CachePolicies.NO_CACHE,
  });

  const updateDevice = useCallback(
    ({ identifier }: {identifier: DeviceIdentifier}) => {
      const payload: { [key: string]: DeviceIdentifier[] } = {
        connectedDeviceIds: [],
        disconnectedDeviceIds: [],
      };

      if (deviceData.connectedDevices.some((item: DeviceInfo) => areDeviceTypesEqual(item.identifier, identifier))) {
        payload.disconnectedDeviceIds.push(identifier);
      } else {
        payload.connectedDeviceIds.push(identifier);
      }

      post(ApiPaths.User, payload).then(() => {
        get(ApiPaths.User);
      });
    },
    [deviceData, post, get]
  );

  useEffect(() => {
    async function getUser() {
      await get(ApiPaths.User);
    }
    getUser();
  }, [get]);

  return {
    deviceData,
    loadingDevices,
    loadingDevicesError,
    updateDevice,
    isUpdating,
  };
};
