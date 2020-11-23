import { redirectToWithings } from "auth/withings";
import isEqual from "lodash/isEqual"
import sortBy from "lodash/sortBy"

export interface DeviceIdentifier {
  system: string;
  type: {
    coding: {
      code: string;
      system: string;
    }[];
  };
  value: string
}

export function areDeviceTypesEqual(identifier1: DeviceIdentifier, identifier2: DeviceIdentifier) {
  if (identifier1.system !== identifier2.system) {
    return false;
  }

  return isEqual(sortBy(identifier1.type.coding, ["system", "code"]), sortBy(identifier2.type.coding, ["system", "code"]));
}

export const WithingsDevices: { name: string; identifier: DeviceIdentifier; imageUrl: string; connected: boolean }[] = [
  {
    name: "Wireless Blood Pressure",
    identifier: {
      system: "http://withings.com",
      type: {
        coding: [
          { code: "Blood Pressure Monitor", system: "http://withings.com/device/type" },
          { code: "45", system: "http://withings.com/device/model_id" },
        ],
      },
      value: ''
    },
    imageUrl: "/blood-pressure.png",
    connected: false,
  },
  {
    name: "Body Wi-Fi Scales",
    identifier: {
      system: "http://withings.com",
      type: {
        coding: [
          { code: "Scale", system: "http://withings.com/device/type" },
          { code: "7", system: "http://withings.com/device/model_id" },
        ],
      },
      value: ''
    },
    imageUrl: "/scale.png",
    connected: false,
  },
  {
    name: "Smart Thermometer",
    identifier: {
      system: "http://withings.com",
      type: {
        coding: [
          { code: "Smart Connected Thermometer", system: "http://withings.com/device/type" },
          { code: "70", system: "http://withings.com/device/model_id" },
        ],
      },
      value: ''
    },
    imageUrl: "/thermometer.png",
    connected: false,
  },
];

export const unusedMerchants = {
  name: "Propeller",
  disable: true,
  isMerchantConnected: false,
  logoImageUrl: "/propeller-logo.png",
  connectHandler: null,
  devices: [
    {
      name: "Wireless Blood Pressure",
      imageUrl: "/blood-pressure.png",
    },
    {
      name: "Smart Thermometer",
      imageUrl: "/thermometer.png",
    },
  ],
};

export const StaticList = [
  {
    name: "Withings",
    isMerchantConnected: false,
    logoImageUrl: "/withings-logo.png",
    connectHandler: redirectToWithings,
    devices: [
      {
        name: "Wireless Blood Pressure",
        imageUrl: "/blood-pressure.png",
      },
      {
        name: "Body Wi-Fi Scales",
        imageUrl: "/scale.png",
      },
      {
        name: "Smart Thermometer",
        imageUrl: "/thermometer.png",
      },
    ],
  },
  {
    name: "Propeller",
    isMerchantConnected: false,
    logoImageUrl: "/propeller-logo.png",
    devices: [
      {
        name: "Wireless Blood Pressure",
        imageUrl: "/blood-pressure.png",
      },
      {
        name: "Smart Thermometer",
        imageUrl: "/thermometer.png",
      },
    ],
  },
];
