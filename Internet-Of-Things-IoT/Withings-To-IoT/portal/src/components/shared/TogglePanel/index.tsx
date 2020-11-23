import React, { FC, useCallback, useContext, useState } from "react"
import { TogglePanelItem } from "./TogglePanel.Item"
import { TogglePanelItemChecked } from "./TogglePanel.Item.Checked"

interface TogglePanelProviderProps {
  className?: string
  toggleType: "single" | "multi"
  children?: React.ReactNode
  selectedIndexes?: number[]
}

export interface TogglePanelItemProps {
  index: number
  className?: string
  onClick?: (val: number) => void
  imageUrl: string
  CustomDescriptionComponent: ({ className }: { className?: string }) => React.ReactElement
}

const TogglePaneContext = React.createContext({})

export const TogglePanel: FC<TogglePanelProviderProps> & {
  Item: FC<TogglePanelItemProps>
  ItemChecked: FC<TogglePanelItemProps>
} = props => {
  const [selectedIndexes, setIndexes] = useState<number[]>(props.selectedIndexes || [0])

  const replaceIndex = useCallback((val: number) => {
    setIndexes([val])
  }, [])

  return (
    <TogglePaneContext.Provider
      value={{
        selectedIndexes,
        replaceIndex,
      }}
    >
      <div className={props.className}>{props.children}</div>
    </TogglePaneContext.Provider>
  )
}

TogglePanel.Item = TogglePanelItem
TogglePanel.ItemChecked = TogglePanelItemChecked

export const useTogglePanelContext = () => useContext(TogglePaneContext)
