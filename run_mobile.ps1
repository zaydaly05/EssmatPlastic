# EsmatPlastic - Launch Mobile App with API
$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
Set-Location $root

Write-Host "Checking EsmatPlastic API status on http://localhost:5023..." -ForegroundColor Cyan

$apiRunning = $false
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5023/api/Health/status" -TimeoutSec 3 -ErrorAction SilentlyContinue
    if ($health -and $health.status -eq "Healthy") {
        $apiRunning = $true
        Write-Host "EsmatPlastic API is already running and healthy." -ForegroundColor Green
    }
} catch {
    $apiRunning = $false
}

if (-not $apiRunning) {
    Write-Host "Starting EsmatPlastic.API in background..." -ForegroundColor Yellow
    Start-Process dotnet -ArgumentList "run --project '$root/EsmatPlastic.API/EsmatPlastic.API.csproj' --urls http://localhost:5023" -WorkingDirectory $root

    $attempts = 0
    while (-not $apiRunning -and $attempts -lt 15) {
        Start-Sleep -Seconds 2
        $attempts++
        try {
            $health = Invoke-RestMethod -Uri "http://localhost:5023/api/Health/status" -TimeoutSec 3 -ErrorAction SilentlyContinue
            if ($health -and $health.status -eq "Healthy") {
                $apiRunning = $true
                Write-Host "EsmatPlastic API is up and running on http://localhost:5023!" -ForegroundColor Green
            }
        } catch {}
    }

    if (-not $apiRunning) {
        Write-Error "Failed to start EsmatPlastic.API within 30 seconds."
        exit 1
    }
}

Write-Host "Launching Flutter Mobile App..." -ForegroundColor Cyan
$env:ANDROID_PREFS_ROOT = $null
Set-Location "$root/esmat_plastic_mobile"
flutter run
