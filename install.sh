#!/usr/bin/env sh
# apigen dotnet CLI installer  —  macOS, Linux, WSL
#
#   curl -fsSL https://raw.githubusercontent.com/apiaddicts/apigen.net/main/install.sh | sh
#
# Optional variables:
#   APIGEN_VERSION=1.0.2    # install a specific version (default: latest)
#   APIGEN_INSTALL_DIR=...  # install directory (default: $HOME/.apigen/bin)

set -eu

# --- Configuration -----------------------------------------------------------
REPO="apiaddicts/apigen.net"
VERSION="${APIGEN_VERSION:-latest}"
INSTALL_DIR="${APIGEN_INSTALL_DIR:-$HOME/.apigen/bin}"
BIN_NAME="apigen"

# --- Platform detection ------------------------------------------------------
os="$(uname -s)"
arch="$(uname -m)"

case "$os" in
    Linux)  platform="linux-x64" ;;
    Darwin)
        case "$arch" in
            arm64|aarch64) platform="osx-arm64" ;;
            x86_64) echo "Intel Macs (osx-x64) are not supported. Use an Apple Silicon Mac, or build from source." >&2; exit 1 ;;
            *) echo "Unsupported macOS architecture: $arch" >&2; exit 1 ;;
        esac
        ;;
    *) echo "Unsupported operating system: $os" >&2; exit 1 ;;
esac

asset="apigen-dotnet-cli-${platform}"

# --- Download URL ------------------------------------------------------------
if [ "$VERSION" = "latest" ]; then
    url="https://github.com/${REPO}/releases/latest/download/${asset}"
else
    url="https://github.com/${REPO}/releases/download/${VERSION}/${asset}"
fi

echo "==> Downloading apigen ($platform, $VERSION)..."
mkdir -p "$INSTALL_DIR"
target="$INSTALL_DIR/$BIN_NAME"

if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$url" -o "$target"
elif command -v wget >/dev/null 2>&1; then
    wget -qO "$target" "$url"
else
    echo "curl or wget is required." >&2
    exit 1
fi

chmod +x "$target"
echo "    Installed at: $target"

# --- PATH --------------------------------------------------------------------
case ":$PATH:" in
    *":$INSTALL_DIR:"*) ;;  # already on PATH
    *)
        echo ""
        echo "==> Add this folder to your PATH (append it to your ~/.bashrc, ~/.zshrc, etc.):"
        echo "    export PATH=\"$INSTALL_DIR:\$PATH\""
        ;;
esac

echo ""
echo "==> Done. Run:  apigen --help"
