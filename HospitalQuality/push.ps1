param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Message
)

$currentDir = Get-Location
$gitRoot = $null

while ($null -ne $currentDir) {
    if (Test-Path (Join-Path $currentDir ".git")) {
        $gitRoot = $currentDir
        break
    }
    $currentDir = Split-Path $currentDir -Parent
}

if ($null -eq $gitRoot) {
    Write-Host "Error: .git folder not found in any parent directory." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Git root: $gitRoot" -ForegroundColor Cyan
Write-Host "Starting push to GitHub..." -ForegroundColor Cyan

Push-Location $gitRoot

$status = git status --porcelain
if ([string]::IsNullOrWhiteSpace($status)) {
    Write-Host "No changes to push." -ForegroundColor Yellow
    Pop-Location
    exit 0
}

Write-Host "Changed files:" -ForegroundColor Yellow
git status --short
Write-Host ""

Write-Host "Adding changes..." -ForegroundColor Cyan
git add .

Write-Host "Committing..." -ForegroundColor Cyan
git commit -m $Message

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error in commit." -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "Pushing to GitHub..." -ForegroundColor Cyan
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error in push." -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "Successfully pushed!" -ForegroundColor Green
Write-Host ""