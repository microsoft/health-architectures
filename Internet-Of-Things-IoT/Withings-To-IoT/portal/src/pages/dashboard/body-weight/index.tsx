import moment from "moment";
import React from "react";
import styled from "styled-components";
import { Modal as SingleModal } from "components/shared/modal";
import { PortalWithState } from "react-portal";
import { theme } from "twin.macro";
import { useObservations } from "components/pages/shared/hooks/useObservations";
import { VictoryAxis, VictoryChart, VictoryGroup, VictoryLine, VictoryScatter, VictoryTooltip } from "victory";

export const ChartContainer = styled.div`
  max-width: 968px;
  margin: 0 auto;
  width: 100%;
  height: 100%;
`;
export const ChartFontStyles = { style: { tickLabels: { fontSize: 12 } } };

const Modal = () => {
  const { bodyWeightData } = useObservations();
  if (bodyWeightData.length <= 0) return null;

  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <SingleModal
              title="Body Weight"
              Component={() => {
                return (
                  <ChartContainer>
                    <VictoryChart domainPadding={10} scale={{ x: "time" }}>
                      <VictoryGroup data={bodyWeightData}>
                        <VictoryAxis dependentAxis tickFormat={tick => `${Math.round(tick)} lb`} {...ChartFontStyles} />
                        <VictoryAxis
                          crossAxis
                          tickFormat={tick => `${moment(tick).format("M-D")}`}
                          style={{ tickLabels: { fontSize: 8 } }}
                          fixLabelOverlap={true}
                        />
                        <VictoryScatter
                          style={{ data: { fill: `${theme`colors.blue`}` } }}
                          labelComponent={<VictoryTooltip activateData={true} />}
                          labels={({ datum }) => `${moment(datum.x).format("l")}\n${datum.y} lb`}
                        />
                        <VictoryLine
                          style={{
                            data: { stroke: `${theme`colors.blue`}`, strokeWidth: 1 },
                            labels: { fontSize: 8 },
                          }}
                        />
                      </VictoryGroup>
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
