#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

function ReadJsonc
{
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $Path
  )

  return $($(Get-Content $Path | Select-String -NotMatch -Pattern '^\s*//.*$') -join "`n").Trim()
}

function ReadXml
{
  param (
    [Parameter(Mandatory=$true)]
    [string]
    $Path
  )

  return $($(Get-Content $Path | Select-String -NotMatch -Pattern '^\s*<!--.*-->$') -join "`n").Trim()
}

function Templatize
{
  param (
    [Parameter(Mandatory=$true)]
    [PSObject]
    $Deployment,

    [Parameter(Mandatory=$true)]
    [string]
    $Text
  )

  foreach ($output in $Deployment.Properties)
  {
    $Text = $Text.Replace("{{ $($output.Name) }}", $output.Value.value)
  }

  return $Text
}
