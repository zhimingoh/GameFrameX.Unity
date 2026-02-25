#!/usr/bin/env bash
set -euo pipefail

UNITY_BIN="${UNITY_PATH:-unity}"
PROJECT_PATH="${PROJECT_PATH:-$(pwd)}"
LOG_FILE="${LOG_FILE:-Build/ci.log}"

mkdir -p "$(dirname "$LOG_FILE")"

"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod CI.BuildWebGL.Build \
  -logFile "$LOG_FILE"
