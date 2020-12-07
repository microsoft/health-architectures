import moment from "moment";
import React, { useMemo } from "react";
import { Candle, VictoryAxis, VictoryCandlestick, VictoryChart, VictoryTooltip } from "victory";
import { ChartContainer, ChartFontStyles } from "../body-weight";
import { Modal as SingleModal } from "components/shared/modal";
import { PortalWithState } from "react-portal";
import { theme } from "twin.macro";
import { useObservations } from "components/pages/shared/hooks/useObservations";

const Custom: React.FC<Partial<{
  fill: string;
  style: any;
  width: number;
  x: number;
  y: number;
  height: number;
  isSingleMeasurement: boolean;
}>> = props => {
  return (
    <rect
      vectorEffect="non-scaling-stroke"
      rx="3"
      {...props}
      style={{
        fill: `${theme`colors.blue`}`,
        stroke: `${theme`colors.blue`}`,
        strokeWidth: 3,
      }}
    />
  );
};

const Labels = (
  datum: {
    isSingleMeasurement: boolean;
    high: number;
    low: number;
    x: moment.Moment;
  },
  type: "Systolic" | "Diastolic"
) => {
  const formattedDate = moment(datum.x).format("l");
  if (datum.isSingleMeasurement) {
    return `${formattedDate}\n${type}: ${datum.high}`;
  } else {
    return `${moment(datum.x).format("l")}\n${type} max: ${datum.high}\n${type} min: ${datum.low}`;
  }
};

const Modal = () => {
  const candlestickProps = useMemo(() => {
    return {
      style: { closeLabels: { fontSize: 5 }, lowLabels: { fontSize: 5 } },
      candleWidth: 5,
      wickStrokeWidth: 1,
      candleColors: { positive: `${theme`colors.blue`}` },
      dataComponent: <Candle rectComponent={<Custom />} />,
      labelComponent: <VictoryTooltip />,
    };
  }, []);
  const { bloodPressureData } = useObservations();
  if (bloodPressureData[0].length <= 0) return null;

  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <SingleModal
              title="Blood Pressure (mmHg)"
              Component={() => {
                return (
                  <ChartContainer>
                    <VictoryChart domainPadding={{ x: 25, y: 5 }} scale={{ x: "time" }}>
                      <VictoryAxis dependentAxis tickFormat={tick => `${moment(tick)}`} {...ChartFontStyles} />
                      <VictoryAxis
                        crossAxis
                        tickFormat={tick => `${moment(tick).format("M-D")}`}
                        style={{ tickLabels: { fontSize: 8 } }}
                        fixLabelOverlap={true}
                      />
                      <VictoryCandlestick
                        {...candlestickProps}
                        data={bloodPressureData[0]}
                        labels={({ datum }) => Labels(datum, "Systolic")}
                      />
                      <VictoryCandlestick
                        {...candlestickProps}
                        data={bloodPressureData[1]}
                        labels={({ datum }) => Labels(datum, "Diastolic")}
                      />
                    </VictoryChart>
                  </ChartContainer>
                );
              }}
            />
          )}
        </React.Fragment>
      )}
    </PortalWithState>
  );
};

export default Modal;
