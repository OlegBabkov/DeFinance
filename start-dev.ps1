param(
    [switch]$NoFrontend   # skip launching Vite if you only want the containers
)

$ComposeFile = Join-Path $PSScriptRoot "docker\docker-compose.yml"
$WebDir      = Join-Path $PSScriptRoot "src\DeFinance.Web"

# Start (or resume) containers in detached mode
Write-Host "Starting Docker containers..." -ForegroundColor Cyan
docker compose -f $ComposeFile up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "docker compose failed — aborting." -ForegroundColor Red
    exit 1
}

if ($NoFrontend) {
    Write-Host "Containers started. Skipping frontend dev server (-NoFrontend)." -ForegroundColor Yellow
    exit 0
}

# Wait for postgres (the minimum the API needs) to be healthy
Write-Host "Waiting for postgres to be healthy..." -ForegroundColor Cyan
$maxWait = 60   # seconds
$elapsed = 0
while ($elapsed -lt $maxWait) {
    $status = docker inspect --format "{{.State.Health.Status}}" definance-postgres 2>$null
    if ($status -eq "healthy") { break }
    Start-Sleep -Seconds 2
    $elapsed += 2
}
if ($elapsed -ge $maxWait) {
    Write-Host "Postgres did not become healthy within ${maxWait}s — containers are still starting, continuing anyway." -ForegroundColor Yellow
}

# Launch Vite dev server in a new PowerShell window
Write-Host "Launching Vite dev server in a new window..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList '-NoExit', '-Command', "cd '$WebDir'; npm run dev"

Write-Host "Done. Frontend dev server is starting at http://localhost:5173" -ForegroundColor Green
