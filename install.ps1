# apigen dotnet CLI installer  —  Windows PowerShell
#
#   irm https://raw.githubusercontent.com/apiaddicts/apigen.net/main/install.ps1 | iex
#
# Optional variables (set before invoking):
#   $env:APIGEN_VERSION     = "1.0.2"     # specific version (default: latest)
#   $env:APIGEN_INSTALL_DIR = "C:\..."    # install directory

$ErrorActionPreference = "Stop"

# --- Configuration -----------------------------------------------------------
$Repo    = "apiaddicts/apigen.net"
$Version = if ($env:APIGEN_VERSION) { $env:APIGEN_VERSION } else { "latest" }
$InstallDir = if ($env:APIGEN_INSTALL_DIR) { $env:APIGEN_INSTALL_DIR } else { "$env:LOCALAPPDATA\apigen\bin" }
$BinName = "apigen.exe"

# --- Platform (win-x64 only for now) -----------------------------------------
$asset = "apigen-dotnet-cli-win-x64.exe"

# --- Download URL ------------------------------------------------------------
if ($Version -eq "latest") {
    $url = "https://github.com/$Repo/releases/latest/download/$asset"
} else {
    $url = "https://github.com/$Repo/releases/download/$Version/$asset"
}

Write-Host "==> Downloading apigen (win-x64, $Version)..."
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
$target = Join-Path $InstallDir $BinName

Invoke-WebRequest -Uri $url -OutFile $target -UseBasicParsing
Write-Host "    Installed at: $target"

# --- PATH (user) -------------------------------------------------------------
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$InstallDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$userPath;$InstallDir", "User")
    Write-Host ""
    Write-Host "==> Added to your user PATH. Restart the terminal for it to take effect."
}

Write-Host ""
Write-Host "==> Done. Run:  apigen --help"
