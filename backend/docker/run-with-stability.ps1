param(
    [string]$Key
)

function PromptForKey {
    Write-Host "Enter your Stability API key (will not be saved):"
    $secure = Read-Host -AsSecureString
    return [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
}

if (-not $Key) {
    if ($env:STABILITY_API_KEY) {
        $Key = $env:STABILITY_API_KEY
    } else {
        $Key = PromptForKey
    }
}

if (-not $Key) {
    Write-Error "No Stability API key provided. Aborting."
    exit 1
}

# Set env var for current process so docker compose picks it up
$env:STABILITY_API_KEY = $Key

Write-Host "Starting docker compose with Stability enabled..."

Push-Location -Path (Join-Path $PSScriptRoot '.')
try {
    docker compose up --build -d
    if ($LASTEXITCODE -ne 0) {
        Write-Error "docker compose failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "Containers started. Following logs for 'api' and 'worker' (Ctrl+C to exit)..."
    docker compose logs --follow api worker
}
finally {
    Pop-Location
}
