param(
    [Parameter(Mandatory)][string]$Message,
    [string[]]$Files = @()
)

if ($Files.Count -gt 0) {
    git add $Files
} else {
    git add -A
}

git commit -m $Message
