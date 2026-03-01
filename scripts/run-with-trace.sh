#!/usr/bin/env bash
#
# run-with-trace.sh — Run a sample/showcase app and collect an EventPipe trace.
#
# Usage:
#   ./scripts/run-with-trace.sh <app-name> [-c Configuration] [-p Providers] [-- app args...]
#
# Examples:
#   ./scripts/run-with-trace.sh SandboxApp
#   ./scripts/run-with-trace.sh AlohaAI
#   ./scripts/run-with-trace.sh AlohaAI -c Release
#   ./scripts/run-with-trace.sh SandboxApp -p gc-collect
#   ./scripts/run-with-trace.sh BenchmarkApp -- --test AlohaTabBarNavigationLeak
#   ./scripts/run-with-trace.sh list
#
# The trace (.nettrace) is saved to diagnostics/<app>_<timestamp>.nettrace
# You can open it in Visual Studio, PerfView, speedscope (https://speedscope.app),
# or convert with: dotnet-trace convert <file> --format Chromium
#
# Prerequisites:
#   dotnet tool install -g dotnet-trace
#

set -eo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DIAG_DIR="$REPO_ROOT/diagnostics"

# ── Provider presets ─────────────────────────────────────────────────────────
# Each preset maps to a dotnet-trace --providers string.
# Use -p <preset> to select one, or -p custom:... to pass a raw provider string.
PRESET_NAMES=(
    default
    gc-verbose
    gc-collect
    gc-alloc
    cpu
    exceptions
    contentions
    events
)

PRESET_PROVIDERS=(
    # default: GC events + runtime informational
    "Microsoft-DotNETCore-SampleProfiler,Microsoft-Windows-DotNETRuntime:0x4C14FCCBD:4"
    # gc-verbose: Full GC detail including heap survival and movement
    "Microsoft-Windows-DotNETRuntime:0x1:5"
    # gc-collect: GC collection events only (lightweight)
    "Microsoft-Windows-DotNETRuntime:0x1:4"
    # gc-alloc: GC + allocation tick events
    "Microsoft-Windows-DotNETRuntime:0x21:4"
    # cpu: CPU sampling profiler
    "Microsoft-DotNETCore-SampleProfiler,Microsoft-Windows-DotNETRuntime:0x4C14FCCBD:4"
    # exceptions: Exception events
    "Microsoft-Windows-DotNETRuntime:0x8000:4"
    # contentions: Lock contention events
    "Microsoft-Windows-DotNETRuntime:0x4000:4"
    # events: All runtime events at informational level
    "Microsoft-Windows-DotNETRuntime::4"
)

# ── App registry ─────────────────────────────────────────────────────────────
# Parallel arrays (Bash 3.x compatible — no associative arrays on macOS default bash).
APP_NAMES=(
    # Samples
    SandboxApp
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
    samples/SandboxApp/SandboxApp.csproj
    samples/AvaloniaSandboxApp/AvaloniaSandboxApp.csproj
    samples/ControlGallery/ControlGallery.Desktop/ControlGallery.Desktop.csproj
    samples/Controls.Sample/Controls.Sample.Desktop/Maui.Controls.Sample.Desktop.csproj
    samples/BenchmarkApp/BenchmarkApp.Desktop/BenchmarkApp.Desktop.csproj
    showcase/2048Game/2048Game.Desktop/2048Game.Desktop.csproj
    showcase/AlohaKit.Gallery/AlohaKit.Gallery.Desktop/AlohaKit.Gallery.Desktop.csproj
    showcase/AlohaAI/AlohaAI.Desktop/AlohaAI.Desktop.csproj
    showcase/MauiPlanets/MauiPlanets.Desktop/MauiPlanets.Desktop.csproj
    showcase/WeatherTwentyOne/WeatherTwentyOne.Desktop/WeatherTwentyOne.Desktop.csproj
)

# ── Helpers ──────────────────────────────────────────────────────────────────
usage() {
    echo "Usage: $0 <app-name> [-c Configuration] [-p Preset] [-- app args...]"
    echo ""
    echo "Run a sample/showcase app and collect an EventPipe trace."
    echo "The app binary is executed directly (not via 'dotnet run') so that"
    echo "dotnet-trace captures the correct process."
    echo ""
    echo "Available apps:"
    list_apps
    echo ""
    echo "Options:"
    echo "  -c CONFIG    Build configuration (default: Debug)"
    echo "  -p PRESET    Provider preset (default: default)"
    echo "  -- ARGS      Arguments passed to the app binary"
    echo "  list         List available apps"
    echo "  presets      List available provider presets"
    echo "  --help, -h   Show this help"
    echo ""
    echo "Provider presets:"
    list_presets
    echo ""
    echo "  Use -p custom:<providers> to pass a raw provider string, e.g.:"
    echo "  $0 SandboxApp -p 'custom:Microsoft-Windows-DotNETRuntime:0x1:5'"
    echo ""
    echo "Examples:"
    echo "  $0 SandboxApp"
    echo "  $0 AlohaAI -c Release"
    echo "  $0 SandboxApp -p gc-alloc"
    echo "  $0 BenchmarkApp -- --test AlohaTabBarNavigationLeak"
    echo ""
    echo "After collection, convert with:"
    echo "  dotnet-trace convert <file>.nettrace --format Chromium"
    echo "  dotnet-trace convert <file>.nettrace --format Speedscope"
}

list_apps() {
    printf "  %-22s %s\n" "NAME" "PROJECT"
    printf "  %-22s %s\n" "----" "-------"
    for i in "${!APP_NAMES[@]}"; do
        printf "  %-22s %s\n" "${APP_NAMES[$i]}" "${APP_PROJECTS[$i]}"
    done
}

list_presets() {
    printf "  %-16s %s\n" "PRESET" "PROVIDERS"
    printf "  %-16s %s\n" "------" "---------"
    for i in "${!PRESET_NAMES[@]}"; do
        printf "  %-16s %s\n" "${PRESET_NAMES[$i]}" "${PRESET_PROVIDERS[$i]}"
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

resolve_providers() {
    local preset="$1"

    # Handle custom:... passthrough
    if [[ "$preset" == custom:* ]]; then
        echo "${preset#custom:}"
        return 0
    fi

    for i in "${!PRESET_NAMES[@]}"; do
        if [[ "${PRESET_NAMES[$i]}" == "$preset" ]]; then
            echo "${PRESET_PROVIDERS[$i]}"
            return 0
        fi
    done
    return 1
}

check_tool() {
    if ! command -v dotnet-trace &>/dev/null; then
        echo "ERROR: dotnet-trace is not installed."
        echo "Install it with: dotnet tool install -g dotnet-trace"
        exit 1
    fi
}

# ── Main ─────────────────────────────────────────────────────────────────────
if [[ $# -lt 1 ]] || [[ "$1" == "--help" ]] || [[ "$1" == "-h" ]]; then
    usage
    exit 0
fi

if [[ "$1" == "list" ]]; then
    list_apps
    exit 0
fi

if [[ "$1" == "presets" ]]; then
    list_presets
    exit 0
fi

APP_NAME="$1"
shift

# Parse options
BUILD_CONFIG="Debug"
PROVIDER_PRESET="default"
APP_ARGS=()

while [[ $# -gt 0 ]]; do
    case "$1" in
        -c)
            BUILD_CONFIG="$2"
            shift 2
            ;;
        -p)
            PROVIDER_PRESET="$2"
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

# Resolve provider string
PROVIDERS=$(resolve_providers "$PROVIDER_PRESET") || {
    echo "ERROR: Unknown provider preset '$PROVIDER_PRESET'"
    echo ""
    echo "Available presets:"
    list_presets
    exit 1
}

PROJECT_PATH="$REPO_ROOT/$PROJECT"

if [[ ! -f "$PROJECT_PATH" ]]; then
    echo "ERROR: Project file not found: $PROJECT"
    exit 1
fi

check_tool
mkdir -p "$DIAG_DIR"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
TRACE_FILE="$DIAG_DIR/${APP_NAME}_${TIMESTAMP}.nettrace"

echo "=== run-with-trace ==="
echo "App:       $APP_NAME"
echo "Config:    $BUILD_CONFIG"
echo "Project:   $PROJECT"
echo "Preset:    $PROVIDER_PRESET"
echo "Providers: $PROVIDERS"
echo "Trace:     diagnostics/${APP_NAME}_${TIMESTAMP}.nettrace"
echo ""

# Build the project
echo "Building $APP_NAME ($BUILD_CONFIG)..."
dotnet build "$PROJECT_PATH" -c "$BUILD_CONFIG" -v q --nologo
echo ""

# Resolve the compiled binary path via MSBuild so we run it directly.
# 'dotnet run' spawns a child process — its PID is the CLI host, not the app,
# so dotnet-trace would capture the wrong (MSBuild) process.
TARGET_PATH=$(dotnet msbuild "$PROJECT_PATH" -getProperty:TargetPath -p:Configuration="$BUILD_CONFIG" -nologo 2>/dev/null | tr -d '[:space:]')

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

TRACE_PID=""

cleanup() {
    echo ""

    # Stop trace collection if running
    if [[ -n "$TRACE_PID" ]] && kill -0 "$TRACE_PID" 2>/dev/null; then
        echo "Stopping trace collection..."
        # dotnet-trace listens for SIGINT to gracefully finalize the trace file
        kill -INT "$TRACE_PID" 2>/dev/null || true
        wait "$TRACE_PID" 2>/dev/null || true
    fi

    # If the app is still running, let it finish
    if kill -0 "$APP_PID" 2>/dev/null; then
        echo "Waiting for app to exit..."
        wait "$APP_PID" 2>/dev/null || true
    fi

    if [[ -f "$TRACE_FILE" ]]; then
        SIZE=$(du -h "$TRACE_FILE" | cut -f1)
        echo ""
        echo "Trace saved: $TRACE_FILE ($SIZE)"
        echo ""
        echo "To analyze:"
        echo "  dotnet-trace convert \"$TRACE_FILE\" --format Speedscope"
        echo "  dotnet-trace convert \"$TRACE_FILE\" --format Chromium"
        echo "  # Then open in https://speedscope.app or chrome://tracing"
    else
        echo "No trace file was produced."
    fi

    echo "Done."
}

trap cleanup EXIT

# Start trace collection
echo "Starting trace collection..."
dotnet-trace collect -p "$APP_PID" --providers "$PROVIDERS" -o "$TRACE_FILE" &
TRACE_PID=$!

sleep 1

if ! kill -0 "$TRACE_PID" 2>/dev/null; then
    echo "ERROR: dotnet-trace failed to start. Check that the app is still running."
    exit 1
fi

echo "Trace collection active (dotnet-trace PID: $TRACE_PID)."
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Trace is recording. Close the app window or press"
echo "  Ctrl+C to stop collection and save the trace."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Wait for the app to exit — trace stops automatically in the cleanup trap
wait "$APP_PID" 2>/dev/null || true
echo "App has exited."
