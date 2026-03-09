#!/usr/bin/env bash
#
# run-with-gcdump.sh — Run a sample/showcase app and collect a GC heap dump.
#
# Usage:
#   ./scripts/run-with-gcdump.sh <app-name> [-c Configuration] [-- app args...]
#
# Examples:
#   ./scripts/run-with-gcdump.sh MauiSandboxApp
#   ./scripts/run-with-gcdump.sh AlohaAI
#   ./scripts/run-with-gcdump.sh AlohaAI -c Release
#   ./scripts/run-with-gcdump.sh BenchmarkApp -- --test AlohaTabBarNavigationLeak
#   ./scripts/run-with-gcdump.sh list
#
# The heap dump (.gcdump) is saved to diagnostics/<app>_<timestamp>.gcdump
# You can open it in Visual Studio, PerfView, or share it for analysis.
#
# Prerequisites:
#   dotnet tool install -g dotnet-gcdump
#

set -eo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DIAG_DIR="$REPO_ROOT/diagnostics"

# ── App registry ──────────────────────────────────────────────────────────────
# Parallel arrays (Bash 3.x compatible — no associative arrays on macOS default bash).
# Apps marked with "multi" in APP_TFMS are multi-TFM single-project apps that
# need -f net11.0 passed to dotnet build/msbuild.
APP_NAMES=(
    # Samples
    MauiSandboxApp
    AvaloniaSandboxApp
    ControlGallery
    ControlsSample
    BenchmarkApp
    # Showcase
    2048Game
    AlohaKit
    AlohaAI
    MauiPlanets
    WeatherTwentyOne
)

APP_PROJECTS=(
    samples/MauiSandboxApp/MauiSandboxApp/MauiSandboxApp.csproj
    samples/AvaloniaSandboxApp/AvaloniaSandboxApp.csproj
    samples/ControlGallery/ControlGallery/ControlGallery.csproj
    samples/Controls.Sample/Controls.Sample/Maui.Controls.Sample.csproj
    samples/BenchmarkApp/BenchmarkApp.Desktop/BenchmarkApp.Desktop.csproj
    showcase/2048Game/2048Game/2048Game.csproj
    showcase/AlohaKit.Gallery/AlohaKit.Gallery/AlohaKit.Gallery.csproj
    showcase/AlohaAI/AlohaAI/AlohaAI.csproj
    showcase/MauiPlanets/MauiPlanets/MauiPlanets.csproj
    showcase/WeatherTwentyOne/WeatherTwentyOne/WeatherTwentyOne.csproj
)

# "multi" = multi-TFM project, needs -f net11.0; "single" = single-TFM, no flag needed
APP_TFMS=(
    multi
    single
    multi
    multi
    single
    multi
    multi
    multi
    multi
    multi
)

# ── Helpers ───────────────────────────────────────────────────────────────────
usage() {
    echo "Usage: $0 <app-name> [-c Configuration] [-- app args...]"
    echo ""
    echo "Run a sample/showcase app and collect a GC heap dump while it runs."
    echo "The app binary is executed directly (not via 'dotnet run') so that"
    echo "dotnet-gcdump captures the correct process."
    echo ""
    echo "Available apps:"
    list_apps
    echo ""
    echo "Options:"
    echo "  -c CONFIG   Build configuration (default: Debug)"
    echo "  -- ARGS     Arguments passed to the app binary"
    echo "  list        List available apps"
    echo "  --help, -h  Show this help"
    echo ""
    echo "Examples:"
    echo "  $0 MauiSandboxApp"
    echo "  $0 AlohaAI -c Release"
    echo "  $0 BenchmarkApp -- --test AlohaTabBarNavigationLeak"
}

list_apps() {
    printf "  %-22s %s\n" "NAME" "PROJECT"
    printf "  %-22s %s\n" "----" "-------"
    for i in "${!APP_NAMES[@]}"; do
        printf "  %-22s %s\n" "${APP_NAMES[$i]}" "${APP_PROJECTS[$i]}"
    done
}

find_project() {
    local name="$1"
    for i in "${!APP_NAMES[@]}"; do
        if [[ "${APP_NAMES[$i]}" == "$name" ]]; then
            echo "${APP_PROJECTS[$i]}"
            return 0
        fi
    done
    return 1
}

find_tfm() {
    local name="$1"
    for i in "${!APP_NAMES[@]}"; do
        if [[ "${APP_NAMES[$i]}" == "$name" ]]; then
            echo "${APP_TFMS[$i]}"
            return 0
        fi
    done
    return 1
}

check_tool() {
    if ! command -v dotnet-gcdump &>/dev/null; then
        echo "ERROR: dotnet-gcdump is not installed."
        echo "Install it with: dotnet tool install -g dotnet-gcdump"
        exit 1
    fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
if [[ $# -lt 1 ]] || [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
    usage
    exit 0
fi

if [[ "$1" == "list" ]]; then
    list_apps
    exit 0
fi

APP_NAME="$1"
shift

# Parse options
BUILD_CONFIG="Debug"
APP_ARGS=()

while [[ $# -gt 0 ]]; do
    case "$1" in
        -c)
            BUILD_CONFIG="$2"
            shift 2
            ;;
        --)
            shift
            APP_ARGS=("$@")
            break
            ;;
        *)
            echo "ERROR: Unknown option '$1'. Use '--' to pass arguments to the app."
            exit 1
            ;;
    esac
done

# Validate app name
PROJECT=$(find_project "$APP_NAME") || {
    echo "ERROR: Unknown app '$APP_NAME'"
    echo ""
    echo "Available apps:"
    list_apps
    exit 1
}

PROJECT_PATH="$REPO_ROOT/$PROJECT"
APP_TFM=$(find_tfm "$APP_NAME")

# Build extra args for multi-TFM projects (need explicit -f net11.0)
TFM_ARGS=()
if [[ "$APP_TFM" == "multi" ]]; then
    TFM_ARGS=(-f net11.0)
fi

if [[ ! -f "$PROJECT_PATH" ]]; then
    echo "ERROR: Project file not found: $PROJECT"
    exit 1
fi

check_tool
mkdir -p "$DIAG_DIR"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DUMP_FILE="$DIAG_DIR/${APP_NAME}_${TIMESTAMP}.gcdump"

echo "=== run-with-gcdump ==="
echo "App:     $APP_NAME"
echo "Config:  $BUILD_CONFIG"
echo "Project: $PROJECT"
echo "Dump:    diagnostics/${APP_NAME}_${TIMESTAMP}.gcdump"
echo ""

# Build the project
echo "Building $APP_NAME ($BUILD_CONFIG)..."
dotnet build "$PROJECT_PATH" -c "$BUILD_CONFIG" "${TFM_ARGS[@]}" -v q --nologo
echo ""

# Resolve the compiled binary path via MSBuild so we run it directly.
# 'dotnet run' spawns a child process — its PID is the CLI host, not the app,
# so dotnet-gcdump would capture the wrong (MSBuild) process.
TARGET_PATH=$(dotnet msbuild "$PROJECT_PATH" -getProperty:TargetPath -p:Configuration="$BUILD_CONFIG" "${TFM_ARGS[@]}" -nologo 2>/dev/null | tr -d '[:space:]')

if [[ -z "$TARGET_PATH" || ! -f "$TARGET_PATH" ]]; then
    echo "ERROR: Could not determine build output path (got: '$TARGET_PATH')."
    echo "Try building manually first: dotnet build $PROJECT -c $BUILD_CONFIG"
    exit 1
fi

# Derive the app-host executable path (same name without .dll extension).
# On macOS/Linux the app host is the native executable next to the DLL.
APP_HOST="${TARGET_PATH%.dll}"

if [[ -x "$APP_HOST" ]]; then
    RUN_CMD="$APP_HOST"
    echo "Starting $APP_NAME (app host)..."
else
    # Fallback: run via 'dotnet <path.dll>' if no app host exists
    RUN_CMD="dotnet"
    APP_ARGS=("$TARGET_PATH" "${APP_ARGS[@]}")
    echo "Starting $APP_NAME (dotnet $TARGET_PATH)..."
fi

"$RUN_CMD" "${APP_ARGS[@]}" &
APP_PID=$!

# Give the app a moment to start
sleep 2

# Verify it's actually running
if ! kill -0 "$APP_PID" 2>/dev/null; then
    echo "ERROR: App failed to start."
    exit 1
fi

echo "App running (PID: $APP_PID)."
echo ""

DUMP_COUNT=0

cleanup() {
    echo ""
    # If the app is still running, let it finish
    if kill -0 "$APP_PID" 2>/dev/null; then
        echo "Waiting for app to exit..."
        wait "$APP_PID" 2>/dev/null || true
    fi

    if [[ $DUMP_COUNT -eq 0 ]]; then
        echo "No dumps were collected."
    else
        echo ""
        echo "Collected $DUMP_COUNT dump(s) in: diagnostics/"
        ls -lh "$DIAG_DIR/${APP_NAME}_${TIMESTAMP}"*.gcdump 2>/dev/null || true
    fi
    echo "Done."
}

trap cleanup EXIT

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Press ENTER to collect a GC heap dump."
echo "  You can do this multiple times while the app runs."
echo "  Press Ctrl+C or close the app window to finish."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

while true; do
    # Check if app is still alive
    if ! kill -0 "$APP_PID" 2>/dev/null; then
        echo "App has exited."
        break
    fi

    # Wait for Enter with a timeout so we can check if the app is still alive
    if read -t 2 -r 2>/dev/null; then
        # User pressed Enter
        if ! kill -0 "$APP_PID" 2>/dev/null; then
            echo "App has already exited, cannot collect dump."
            break
        fi

        DUMP_COUNT=$((DUMP_COUNT + 1))
        if [[ $DUMP_COUNT -gt 1 ]]; then
            DUMP_FILE="$DIAG_DIR/${APP_NAME}_${TIMESTAMP}_${DUMP_COUNT}.gcdump"
        fi

        echo "Collecting GC heap dump (#$DUMP_COUNT)..."
        if dotnet-gcdump collect -p "$APP_PID" -o "$DUMP_FILE" 2>&1; then
            SIZE=$(du -h "$DUMP_FILE" | cut -f1)
            echo "Saved: $DUMP_FILE ($SIZE)"
        else
            echo "WARNING: gcdump collection failed (app may have exited)."
        fi
        echo ""
        echo "Press ENTER for another dump, or close the app to finish."
    fi
done
