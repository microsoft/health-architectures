import React, { useState } from "react";
import { PortalWithState } from "react-portal";
import { SidePane } from "components/pages/connect/old-connect-components/modal-side-pane";
import { TogglePanel } from "components/shared/TogglePanel";
import { StyledDualPaneModal, StyledCustomDescription } from "components/pages/connect/old-connect-components";

const Modal = () => {
  const [index, setSelectedIndex] = useState<number>(0);

  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <StyledDualPaneModal
              title="Connect Your Device Account"
              LeftPaneComponent={() => (
                <TogglePanel className="left-pane-container" toggleType="single" selectedIndexes={[index]}>
                  {[
                    {
                      imageUrl: "/garmin-watch.png",
                      title: "Withings",
                      description: "Blood Pressure Monitor & Scale",
                    },
                    {
                      imageUrl: "/sleep-monitor.png",
                      title: "Garmin",
                      description: "Activity Tracker",
                    },
                    {
                      imageUrl: "/garmin-watch.png",
                      title: "Propeller",
                      description: "Smart Inhaler",
                    },
                    {
                      imageUrl: "/sleep-monitor.png",
                      title: "EMFIT",
                      description: "Sleep Monitor",
                    },
                  ].map((item, i) => {
                    return (
                      <TogglePanel.Item
                        key={i}
                        className="panel-item"
                        index={i}
                        onClick={(val: number) => {
                          setSelectedIndex(val);
                        }}
                        CustomDescriptionComponent={() => (
                          <StyledCustomDescription title={item.title} description={item.description} />
                        )}
                        {...item}
                      />
                    );
                  })}
                </TogglePanel>
              )}
              RightPaneComponent={() => {
                return <SidePane className="right-pane-container" selectedIndex={Number(index)} />;
              }}
            />
          )}
        </React.Fragment>
      )}
    </PortalWithState>
  );
};

export default Modal;
