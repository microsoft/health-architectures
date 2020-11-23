import React, { FC } from "react"
import styled from "styled-components"
import { PanelItemStyles } from "components/shared/TogglePanel/TogglePanel.Item.styles"
import { TogglePanelItemProps, useTogglePanelContext } from "components/shared/TogglePanel"
import EnglishStrings from "content/i8ln/en-US.json"
// TODO: Need to add a module for handling i8ln strings

export interface ContextProps {
  selectedIndexes: number[]
  replaceIndex: (val: number) => void
}

export const TogglePanelStyles = (Component: FC<TogglePanelItemProps>) => styled(Component)`
  ${PanelItemStyles}
`

export const TogglePanelItem = TogglePanelStyles(({ className, index, CustomDescriptionComponent, ...props }) => {
  const context = useTogglePanelContext() as ContextProps

  return (
    <div
      className={`${className} ${context.selectedIndexes.includes(index) ? "active" : ""}`}
      onClick={() => {
        props.onClick?.(index)
        context.replaceIndex(index)
      }}
    >
      <img src={props.imageUrl} alt={EnglishStrings["connected.device.image"]} />
      <CustomDescriptionComponent />
    </div>
  )
})
