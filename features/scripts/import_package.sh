#!/usr/bin/env bash

set -e

# === CONFIGURATION ===
FIXTURE_PATH="features/fixtures/mazerunner"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit"
BUGSNAG_RELEASE_URL="https://github.com/bugsnag/bugsnag-unity/releases/latest/download/Bugsnag.unitypackage"
PACKAGE_DOWNLOAD_PATH="$FIXTURE_PATH/Bugsnag.unitypackage"
UPM_PACKAGE_ZIP="upm-package.zip"
PACKAGE_DESTINATION="$FIXTURE_PATH/Packages"

BUILD_WINDOWS=false

# === FUNCTIONS ===

abort() {
  echo "❌ $1"
  exit 1
}

download_bugsnag_package() {
  echo "⬇️ Downloading Bugsnag.unitypackage from $BUGSNAG_RELEASE_URL"
  curl -L "$BUGSNAG_RELEASE_URL" -o "$PACKAGE_DOWNLOAD_PATH"
}

import_package_into_unity() {
  echo "📦 Importing Bugsnag.unitypackage into $FIXTURE_PATH"
  "$UNITY_PATH" $DEFAULT_CLI_ARGS \
    -projectPath "$FIXTURE_PATH" \
    -ignoreCompilerErrors \
    -importPackage "$(basename "$PACKAGE_DOWNLOAD_PATH")"
}

unzip_upm_package() {
  echo "📂 Unzipping UPM package..."
  rm -rf "$PACKAGE_DESTINATION/package"
  unzip -q "$UPM_PACKAGE_ZIP" -d "$PACKAGE_DESTINATION"
}

# === INPUT VALIDATION ===

if [[ -z "${UNITY_PERFORMANCE_VERSION:-}" ]]; then
  abort "UNITY_PERFORMANCE_VERSION must be set"
fi

# === PARSE ARGUMENTS ===

while [[ $# -gt 0 ]]; do
  case "$1" in
    --windows)
      BUILD_WINDOWS=true
      ;;
    *)
      abort "Unknown option: $1"
      ;;
  esac
  shift
done

# === SET UNITY PATH ===

if [[ "$BUILD_WINDOWS" == true ]]; then
  UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/${UNITY_PERFORMANCE_VERSION}/Editor/Unity.exe"
else
  UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_PERFORMANCE_VERSION}/Unity.app/Contents/MacOS/Unity"
fi

if [[ ! -f "$UNITY_PATH" ]]; then
  abort "Unity executable not found at: $UNITY_PATH"
fi

echo "🛠️ Unity path set to: $UNITY_PATH"

# === DOWNLOAD PACKAGE ===
if [[ ! -f "$PACKAGE_DOWNLOAD_PATH" ]]; then
  download_bugsnag_package
else
  echo "ℹ️ Using cached Bugsnag.unitypackage"
fi

# === IMPORT PACKAGE ===
import_package_into_unity

echo "✅ Bugsnag package imported successfully."

# === UNZIP UPM PACKAGE ===

unzip_upm_package

echo "🎉 Package unzipped successfully into $PACKAGE_DESTINATION"
