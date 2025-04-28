#!/usr/bin/env bash

set -e

# === VALIDATE ENVIRONMENT ===
if [[ -z "${UNITY_PERFORMANCE_VERSION:-}" ]]; then
  echo "❌ UNITY_PERFORMANCE_VERSION must be set."
  exit 1
fi

# === VALIDATE INPUT ARGUMENTS ===
if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <build type: dev|release>"
  exit 1
fi

BUILD_TYPE="$1"

if [[ "$BUILD_TYPE" != "dev" && "$BUILD_TYPE" != "release" ]]; then
  echo "❌ Invalid build type: $BUILD_TYPE"
  echo "Allowed values: dev, release"
  exit 1
fi

# === SET PATHS ===
UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_PERFORMANCE_VERSION}/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -nographics -batchmode -logFile generateXcodeProject.log"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIXTURES_DIR="$(cd "$SCRIPT_DIR/../fixtures" && pwd)"
PROJECT_PATH="$FIXTURES_DIR/mazerunner"

echo "📁 Script path: $SCRIPT_DIR"
echo "📁 Fixtures path: $FIXTURES_DIR"
echo "🛠️  Building Xcode project for build type: $BUILD_TYPE"

# === DETERMINE BUILD METHOD ===
if [[ "$BUILD_TYPE" == "dev" ]]; then
  EXECUTE_METHOD="Builder.IosDev"
else
  EXECUTE_METHOD="Builder.IosRelease"
fi

# === RUN UNITY BUILD ===
echo "🚀 Running Unity to generate Xcode project..."
"$UNITY_PATH/Unity" "$DEFAULT_CLI_ARGS" -projectPath "$PROJECT_PATH" -executeMethod "$EXECUTE_METHOD"

echo "✅ Xcode project generated successfully."
