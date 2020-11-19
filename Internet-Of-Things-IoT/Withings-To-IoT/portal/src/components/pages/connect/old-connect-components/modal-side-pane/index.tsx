import React, { useMemo } from "react"
import styled from "styled-components"
import tw from "twin.macro"
import { Button } from "components/shared/Button"

const PaneInfo = styled<
  React.FC<{
    className?: string
    imageUrl: string
    title: string
    description: string
  }>
>(({ className, imageUrl, title, description }) => {
  return (
    <div className={`${className} right-pane-container`}>
      <div className="center">
        <img src={imageUrl} alt="" />
        <div className="text">
          <p className="title">{title}</p>
          <p className="description">{description}</p>
        </div>
        <Button>Connect your account</Button>
      </div>
      <div className="additional-info-pane">
        {/* Hardcode this for now */}
        {[
          {
            title: "Info 1",
            description: "Info detail 1",
          },
          {
            title: "Info 2",
            description: "Info detail 2",
          },
          {
            title: "Info 3",
            description: "Info detail 3",
          },
        ].map((item, i) => (
          <div className="info-wrap" key={i}>
            <p className="info-title">{item.title}</p>
            <p className="info-description">{item.description}</p>
          </div>
        ))}
      </div>
    </div>
  )
})`
  width: 315px;
  .center {
    text-align: center;
    img {
      height: 165px;
      padding: 0.5rem;
      margin-top: 1rem;
    }
    .text {
      line-height: 2rem;
      padding: 1rem;
    }
    .title {
      ${tw`text-large`};
    }
    .description {
      ${tw`text-base`};
    }
  }
  .additional-info-pane {
    padding: 2rem;
    .info-wrap {
      padding: 0.5rem 0;
      line-height: 1.5rem;
      .info-title {
        font-weight: bold;
        ${tw`text-base`};
      }
      .info-description {
        ${tw`text-small`};
      }
    }
  }
`

export const SidePane: React.FC<{
  className: string
  selectedIndex: number
}> = props => {
  const PanelProps = useMemo(
    () => [
      {
        imageUrl: "/garmin-watch.png",
        title: "WITHINGS",
        description: "Blood Pressure Monitor & Scale",
      },
      {
        imageUrl: "/sleep-monitor.png",
        title: "Garmin",
        description: "Garmin Watch",
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
    ],
    []
  )
  return <PaneInfo {...PanelProps[props.selectedIndex]} />
}
