param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Body,
    [string]$Base = "main",
    [string]$Draft = "false"
)

$draftFlag = if ($Draft -eq "true") { "--draft" } else { "" }

if ($draftFlag -ne "") {
    gh pr create --title $Title --body $Body --base $Base $draftFlag
} else {
    gh pr create --title $Title --body $Body --base $Base
}
