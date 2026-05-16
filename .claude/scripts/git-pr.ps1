param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Body,
    [string]$Base = "main",
    [string]$Draft = "false"
)

$gh = if (Get-Command gh -ErrorAction SilentlyContinue) { "gh" }
      elseif (Test-Path "C:\Program Files\GitHub CLI\gh.exe") { "C:\Program Files\GitHub CLI\gh.exe" }
      else { throw "gh CLI not found. Install from https://cli.github.com and run 'gh auth login'." }

$args = @("pr", "create", "--title", $Title, "--body", $Body, "--base", $Base)
if ($Draft -eq "true") { $args += "--draft" }

& $gh @args
