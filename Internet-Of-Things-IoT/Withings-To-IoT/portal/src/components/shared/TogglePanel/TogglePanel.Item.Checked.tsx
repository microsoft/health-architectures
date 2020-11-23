import React from "react"
import { useTogglePanelContext } from "."
import { ContextProps, TogglePanelStyles } from "./TogglePanel.Item"
import classNames from "classnames"

export const TogglePanelItemChecked = TogglePanelStyles(
  ({ className, index, CustomDescriptionComponent, ...props }) => {
    const context = useTogglePanelContext() as ContextProps
    const isConnected = context.selectedIndexes.includes(index)

    return (
      <div
        className={classNames(className, { active: isConnected })}
        onClick={() => {
          if (props.onClick) props.onClick(index)
          context.replaceIndex(index)
        }}
      >
        <img src={props.imageUrl} alt="" />
        <CustomDescriptionComponent className={(isConnected && "show") || "hide"} />
        <p className="connected-text">{isConnected ? "Connected" : "Disconnected"}</p>
      </div>
    )
  }
)
