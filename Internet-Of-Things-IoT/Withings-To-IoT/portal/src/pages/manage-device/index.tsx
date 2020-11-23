import React, { useState } from "react";
import { Button } from "components/shared/Button";
import { PortalWithState } from "react-portal";
import { AlertModal } from "components/shared/alert-modal";
import { StyledTogglePanel } from "components/pages/manage-device-components";
import { TogglePanel } from "components/shared/TogglePanel";

const Modal = () => {
  const [selectedIndexes, setSelectedIndexes] = useState<number[]>([]);
  return (
    <PortalWithState closeOnOutsideClick={false} closeOnEsc={false} defaultOpen={true}>
      {({ portal }) => (
        <React.Fragment>
          {portal(
            <AlertModal
              title="Manage Devices"
              Component={() => (
                <StyledTogglePanel
                  className="manage-device-multi-select"
                  selectedIndexes={selectedIndexes}
                  toggleType="multi"
                >
                  {[
                    {
                      imageUrl: "/garmin-watch.png",
                      title: "Withings",
                      description: "Garmin Watch",
                    },
                    {
                      imageUrl: "/sleep-monitor.png",
                      title: "Garmin",
                      description: "Activity Tracker",
                    },
                  ].map((item, i) => {
                    return (
                      <TogglePanel.ItemChecked
                        key={i}
                        className="panel-item"
                        index={i}
                        onClick={(val: number) => {
                          if (!selectedIndexes.includes(val)) {
                            setSelectedIndexes(old => [...old, val]);
                          } else if (selectedIndexes.includes(val)) {
                            setSelectedIndexes(selectedIndexes.filter(item => item !== val));
                          }
                        }}
                        CustomDescriptionComponent={({ className }) => (
                          <div className={`${className} connected-image`}>
                            <img src="/connected.png" alt="" />
                          </div>
                        )}
                        {...item}
                      />
                    );
                  })}
                  <div className="button-group">
                    <Button>Update</Button>
                    <Button>Cancel</Button>
                  </div>
                </StyledTogglePanel>
              )}
            />
          )}
        </React.Fragment>
      )}
    </PortalWithState>
  );
};

export default Modal;
