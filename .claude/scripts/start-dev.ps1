#!/usr/bin/env pwsh
# Starts all Docker containers and launches the Vite dev server once the API is healthy.

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$dockerDir    = Join-Path $projectRoot "docker"
$webDir       = Join-Path $projectRoot "src\DeFinance.Web"

Write-Host "Starting containers..." -ForegroundColor Cyan
Push-Location $dockerDir
docker compose up -d
Pop-Location

# Poll the API health endpoint until it responds (API starts after postgres/redis/rabbitmq are healthy)
Write-Host "Waiting for API on http://localhost:8080/health ..." -ForegroundColor Cyan
$timeout  = 120   # seconds
$interval = 3
$elapsed  = 0
$ready    = $false

while ($elapsed -lt $timeout) {
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($resp.StatusCode -lt 500) {
            $ready = $true
            Write-Host "API is ready." -ForegroundColor Green
            break
        }
    } catch { }
    Write-Host "  waiting... ($elapsed s elapsed)" -ForegroundColor DarkGray
    Start-Sleep -Seconds $interval
    $elapsed += $interval
}

if (-not $ready) {
    Write-Host "Timed out waiting for API. Check: docker logs definance-api (or docker compose logs api)" -ForegroundColor Red
    exit 1
}

Write-Host "Starting Vite dev server..." -ForegroundColor Cyan
Push-Location $webDir
npm run dev -- --host
Pop-Location
