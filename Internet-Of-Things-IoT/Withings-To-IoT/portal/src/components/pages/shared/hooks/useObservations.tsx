import filter from "lodash/filter";
import first from "lodash/first";
import groupBy from "lodash/groupBy";
import last from "lodash/last";
import moment from "moment";
import uniqBy from "lodash/uniqBy";
import useFetch, { CachePolicies } from "use-http";
import { ApiPaths, getAuthHeaders, getBasePath, removeLocalStorageItems } from "auth/api";
import { navigate } from "gatsby";
import { Paths } from "components/shared/Paths";
import { useEffect, useMemo } from "react";

const CONVERT_TO_LBS_MULTIPLIER = 2.2;
const SINGLE_MEASUREMENT_OFFSET = 2;

export interface Observations {
  data: {
    effectiveDateTime: moment.Moment;
    code: {
      text: "Body Weight" | "Blood Pressure";
    };
    valueQuantity: {
      unit: "kg";
      value: number;
      code: string;
    };
    issued: moment.Moment | string;
  }[];
  bloodPressureData: {
    x: string | moment.Moment;
    open: number;
    close: number;
    high: number;
    low: number;
    isSingleMeasurement: boolean;
  }[][];
  bodyWeightData: {
    x: moment.Moment;
    y: number;
  }[];
  loadingObservations: boolean;
}

export const useObservations = (): Observations => {
  const { get, loading: loadingObservations, error, data = [] } = useFetch<Observations["data"]>(getBasePath(), {
    headers: getAuthHeaders(),
    cachePolicy: CachePolicies.NO_CACHE,
  });

  useEffect(() => {
    async function getObservations() {
      await get(ApiPaths.Observations);
    }

    if (error) {
      // Force user to reconnect to withings
      removeLocalStorageItems(["withingsCode"]);
      navigate(Paths.DeviceConnect);
    }
    getObservations();
  }, [get, error]);

  const sortedData: Observations["data"] = useMemo(() => {
    if (!Array.isArray(data)) {
      return [];
    }
    return data
      .map(item => ({
        ...item,
        effectiveDateTime: (String(item.effectiveDateTime).split("T")[0] as unknown) as moment.Moment,
      }))
      .sort((a, b) => Number(moment(a.effectiveDateTime)) - Number(moment(b.effectiveDateTime)));
  }, [data]);

  const bloodPressureData = useMemo(() => {
    const result: Observations["bloodPressureData"] = [[], []];

    const groupByDate = filter(
      groupBy(sortedData, item => item.effectiveDateTime),
      item => item.length >= 3
    );

    const filterByType = (arr: Observations["data"], type: "systolic blood pressure" | "diastolic blood pressure") => {
      return uniqBy(
        arr
          .filter(item => item.valueQuantity.code === type)
          .sort((a, b) => b.valueQuantity.value - a.valueQuantity.value),
        item => item.valueQuantity.value
      );
    };

    groupByDate.forEach(arr => {
      const getMinMeasurement: (arr: Observations["data"]) => number = arr =>
        (arr.length > 1 && last(arr)!.valueQuantity.value) || arr[0].valueQuantity.value - SINGLE_MEASUREMENT_OFFSET;

      const systolicAll = filterByType(arr, "systolic blood pressure");
      if (systolicAll.length > 0) {
        result[0].push({
          x: first(systolicAll)!.effectiveDateTime,
          high: first(systolicAll)!.valueQuantity.value,
          close: first(systolicAll)!.valueQuantity.value,
          low: getMinMeasurement(systolicAll),
          open: getMinMeasurement(systolicAll),
          isSingleMeasurement: systolicAll.length === 1,
        });
      }

      const diastolicAll = filterByType(arr, "diastolic blood pressure");
      if (diastolicAll.length > 0) {
        result[1].push({
          x: first(diastolicAll)!.effectiveDateTime,
          high: first(diastolicAll)!.valueQuantity.value,
          close: first(diastolicAll)!.valueQuantity.value,
          low: getMinMeasurement(diastolicAll),
          open: getMinMeasurement(diastolicAll),
          isSingleMeasurement: diastolicAll.length === 1,
        });
      }
    });

    return result;
  }, [sortedData]);

  const bodyWeightData = useMemo(() => {
    return sortedData
      .filter(item => item.code.text === "Body Weight")
      .map(item => ({
        x: moment(item.effectiveDateTime),
        y: Math.round(item.valueQuantity.value * CONVERT_TO_LBS_MULTIPLIER),
      }));
  }, [sortedData]);

  return {
    data,
    bloodPressureData,
    bodyWeightData,
    loadingObservations,
  };
};
