#!/usr/bin/env bash
# Builds the apigen CLI for the desktop platforms (win-x64, linux-x64,
# osx-arm64) and drops the self-contained binaries into a local dist/ folder.
#
# Usage:
#   ./build-all.sh                 # builds win-x64, linux-x64, osx-arm64
#   ./build-all.sh win-x64         # builds a single one
#                                  # (win-x64 | linux-x64 | linux-musl-x64 | osx-arm64 | osx-x64)
#
# The binaries are self-contained (embed the .NET runtime) and trimmed.
# They do not require .NET to be installed on the target machine.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/../src/Command"
CSPROJ="$PROJECT_DIR/Command.csproj"
DIST_DIR="$SCRIPT_DIR/../dist"

# ---------------------------------------------------------------------------
# Function: build one platform and copy the binary to dist/
#   $1 = PublishProfile name   $2 = RID   $3 = executable name
# ---------------------------------------------------------------------------
build() {
    local profile="$1"
    local rid="$2"
    local exe_name="$3"

    echo ""
    echo "==> Building $rid (profile $profile)..."
    local publish_dir="$PROJECT_DIR/bin/Release/net10.0/publish/$rid"
    rm -rf "$publish_dir"
    dotnet publish "$CSPROJ" -p:PublishProfile="$profile"

    local src="$publish_dir/$exe_name"
    if [ ! -f "$src" ]; then
        echo "ERROR: expected binary not found: $src" >&2
        exit 1
    fi

    cp "$src" "$DIST_DIR/$exe_name"
    echo "    OK -> dist/$exe_name"
}

# ---------------------------------------------------------------------------
mkdir -p "$DIST_DIR"

TARGET="${1:-all}"

case "$TARGET" in
    win-x64)   build "FolderProfile" "win-x64"   "apigen-dotnet-cli-win-x64.exe" ;;
    linux-x64) build "Linux"         "linux-x64" "apigen-dotnet-cli-linux-x64" ;;
    osx-arm64) build "MacArm64"      "osx-arm64" "apigen-dotnet-cli-osx-arm64" ;;
    osx-x64)   build "MacIntel"      "osx-x64"   "apigen-dotnet-cli-osx-x64" ;;
    all)
        build "FolderProfile" "win-x64"   "apigen-dotnet-cli-win-x64.exe"
        build "Linux"         "linux-x64" "apigen-dotnet-cli-linux-x64"
        build "MacArm64"      "osx-arm64" "apigen-dotnet-cli-osx-arm64"
        ;;
    *)
        echo "ERROR: unknown target '$TARGET'." >&2
        echo "Use: all | win-x64 | linux-x64 | osx-arm64 | osx-x64" >&2
        exit 1
        ;;
esac

echo ""
echo "==> Done. Binaries in: $DIST_DIR"
ls -lh "$DIST_DIR"
