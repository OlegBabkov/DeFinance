param(
    [string]$Branch = ""
)

if ($Branch -eq "") {
    $Branch = git rev-parse --abbrev-ref HEAD
}

git push origin $Branch
