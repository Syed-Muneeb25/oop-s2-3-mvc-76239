param(
  [Parameter(Mandatory = $false)]
  [string]$Root = (Get-Location).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-ProjectReferencePath {
  param(
    [Parameter(Mandatory = $true)][string]$ProjectFile,
    [Parameter(Mandatory = $true)][string]$Include
  )

  $projectDir = Split-Path -Parent $ProjectFile
  $candidate = Join-Path $projectDir $Include

  try {
    return (Resolve-Path -LiteralPath $candidate).Path
  } catch {
    # If it can't be resolved, keep it as a normalized absolute-ish path for reporting.
    return [System.IO.Path]::GetFullPath($candidate)
  }
}

function Get-ProjectGraph {
  param([Parameter(Mandatory = $true)][string]$RootPath)

  $projects = Get-ChildItem -Path $RootPath -Recurse -File -Filter *.csproj |
    Where-Object { $_.FullName -notmatch '\\\\(obj|bin)\\\\' } |
    ForEach-Object { $_.FullName }

  $graph = @{}
  foreach ($p in $projects) {
    $graph[$p] = @()
  }

  foreach ($projectFile in $projects) {
    [xml]$xml = Get-Content -LiteralPath $projectFile

    # Use XPath to avoid strict-mode errors when optional nodes/properties are absent.
    $projectRefNodes = @($xml.SelectNodes("//*[local-name()='ProjectReference']"))
    $refs = @($projectRefNodes | ForEach-Object { $_.GetAttribute('Include') }) | Where-Object { $_ }

    foreach ($include in $refs) {
      $refPath = Resolve-ProjectReferencePath -ProjectFile $projectFile -Include $include
      if ($graph.ContainsKey($refPath)) {
        $graph[$projectFile] += $refPath
      } else {
        # Keep unknown refs for debugging, but don't treat them as nodes.
        $graph[$projectFile] += $refPath
      }
    }
  }

  return $graph
}

function Find-Cycles {
  param([Parameter(Mandatory = $true)][hashtable]$Graph)

  $color = @{}
  $parent = @{}
  foreach ($node in $Graph.Keys) {
    $color[$node] = 'white' # white, gray, black
  }

  $cycles = New-Object System.Collections.Generic.List[string]

  function Build-CycleString {
    param([string]$From, [string]$To)

    $path = New-Object System.Collections.Generic.List[string]
    $path.Add($To) | Out-Null

    $cur = $From
    while ($cur -and $cur -ne $To) {
      $path.Add($cur) | Out-Null
      $cur = $parent[$cur]
    }

    $path.Add($To) | Out-Null
    $arr = $path.ToArray()
    [array]::Reverse($arr)
    return ($arr -join ' -> ')
  }

  function Dfs {
    param([string]$u)

    $color[$u] = 'gray'
    foreach ($v in @($Graph[$u])) {
      if (-not $Graph.ContainsKey($v)) {
        continue
      }

      if ($color[$v] -eq 'white') {
        $parent[$v] = $u
        Dfs -u $v
      } elseif ($color[$v] -eq 'gray') {
        $cycles.Add((Build-CycleString -From $u -To $v)) | Out-Null
      }
    }
    $color[$u] = 'black'
  }

  foreach ($node in $Graph.Keys) {
    if ($color[$node] -eq 'white') {
      $parent[$node] = $null
      Dfs -u $node
    }
  }

  # De-dupe identical cycle strings (same traversal can find same cycle multiple times).
  return $cycles | Sort-Object -Unique
}

$graph = Get-ProjectGraph -RootPath $Root
$cycles = @(Find-Cycles -Graph $graph)

if ($cycles.Count -gt 0) {
  Write-Host "Circular project references detected:" -ForegroundColor Red
  foreach ($c in $cycles) {
    Write-Host "  $c" -ForegroundColor Red
  }
  exit 2
}

Write-Host "No circular project references found." -ForegroundColor Green
exit 0
