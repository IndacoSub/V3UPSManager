param(
    [string]$OutputDirectory = "artifacts/windows-x64"
)
$ErrorActionPreference = "Stop"
$repository = Split-Path $PSScriptRoot -Parent
Push-Location $repository
try {
    $output = [System.IO.Path]::GetFullPath($OutputDirectory)
    # Publish both front ends together with their shared Core dependency.
    dotnet publish V3UPSManager/V3UPSManager.csproj -c Release -r win-x64 --self-contained true -p:EnableWindowsTargeting=true -o $output
    if ($LASTEXITCODE -ne 0) { throw "Manager publication failed." }
    dotnet publish V3UPSWizard/V3UPSWizard.csproj -c Release -r win-x64 --self-contained true -p:EnableWindowsTargeting=true -o $output
    if ($LASTEXITCODE -ne 0) { throw "Wizard publication failed." }
    Write-Host "Published to $output. Add the licensed UPS/XDelta tools and 7za.exe before distribution."
} finally {
    Pop-Location
}
