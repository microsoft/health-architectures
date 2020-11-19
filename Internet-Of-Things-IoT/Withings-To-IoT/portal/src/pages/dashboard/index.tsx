import last from "lodash/last";
import moment from "moment";
import React, { useMemo } from "react";
import styled from "styled-components";
import tw from "twin.macro";
import { HealthPane, Tile, ImageTile } from "components/pages/dashboard";
import { Link } from "gatsby";
import { Modal as SingleModal } from "components/shared/modal";
import { Paths } from "components/shared/Paths";
import { PortalWithState } from "react-portal";
import { useObservations } from "components/pages/shared/hooks/useObservations";

const DashboardPageWrap = styled.div`
  ${tw`flex flex-col w-full p-4 px-8`}
`;

const Modal = () => {
  const { loadingObservations, bodyWeightData, bloodPressureData } = useObservations();

  const summaryList = useMemo(() => {
    const list = [];
    if (bloodPressureData.length === 2 && bloodPressureData[0].length > 0 && bloodPressureData[1].length > 0) {
      list.push({
        type: "Blood Pressure",
        mfg: "Withings",
        alt: "Blood Pressure Monitor",
        lastMeasurement: moment(last(bloodPressureData[0])?.x).format("LT"),
        mainText: `${last(bloodPressureData[0])?.high} / ${last(bloodPressureData[1])?.low}`,
        subText: "mmHg",
        path: Paths.BloodPressureChart,
        imageUrl: "/blood-pressure.png",
      });
    }
    if (bodyWeightData.length > 0) {
      list.push({
        type: "Body Weight",
        mfg: "Withings",
        alt: "Body Wifi Scale",
        lastMeasurement: moment(last(bodyWeightData)?.x).format("LT"),
        mainText: `${last(bodyWeightData)?.y} lbs`,
        subText: "",
        path: Paths.BodyWeightChart,
        imageUrl: "/scale.png",
      });
    }
    return list;
  }, [bodyWeightData, bloodPressureData]);

  if (loadingObservations) return null;

  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <SingleModal
              title="Health Monitoring"
              Component={() => (
                <DashboardPageWrap className="dashboard-page">
                  <HealthPane className="health-pane" headerTitle="Health Summary">
                    <div
                      className="tile-images"
                      css={[tw`flex flex-row flex-wrap mt-0 justify-center md:justify-start`]}
                    >
                      {(summaryList.length > 0 &&
                        summaryList.map((item, i) => {
                          return (
                            <Link to={item.path} key={i}>
                              <Tile height="175" width="210" css={[tw`mx-2 my-2 ml-0 mt-0 md:mr-4 p-4`]}>
                                <div className="info">
                                  <p className="type">{item.type}</p>
                                  <p className="timestamp">Last modified: {item.lastMeasurement}</p>
                                </div>
                                <p className="measurement">{item.mainText}</p>
                                <p className="unit">{item.subText}</p>
                              </Tile>
                            </Link>
                          );
                        })) || <p css={[tw`text-base`]}>No devices connected.</p>}
                    </div>
                  </HealthPane>
                  <HealthPane className="health-pane" headerTitle="Devices">
                    <div className="tile-images" css={[tw`flex flex-row flex-wrap mt-0 justify-center`]}>
                      {summaryList
                        .concat([{ imageUrl: "/gear.png", alt: "Manage Devices", className: "devices" }] as any)
                        .map((item, i) => (
                          <Link to={Paths.DeviceConnect} key={i} css={[tw`mb-4`]}>
                            <ImageTile
                              className="devices"
                              height="160"
                              width="160"
                              css={[tw`mr-4 flex flex-col p-4 items-center`]}
                            >
                              <img src={item.imageUrl} css={[tw`mx-2 my-2 ml-0 mt-0 md:mr-4`]} alt={item.alt} />
                              <p className="brand" css={[tw`text-small`]}>
                                {item.mfg}
                              </p>
                              <p className="description" css={[tw`text-small`]}>
                                {item.alt}
                              </p>
                            </ImageTile>
                          </Link>
                        ))}
                    </div>
                  </HealthPane>
                </DashboardPageWrap>
              )}
            />
          )}
        </React.Fragment>
      )}
    </PortalWithState>
  );
};

export default Modal;
