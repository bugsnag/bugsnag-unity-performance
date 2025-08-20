#!/usr/bin/env bash

set -e

if [ -z "$UNITY_PERFORMANCE_VERSION" ]; then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

# Parse arguments
BUILD_WINDOWS=false
while [[ "$#" -gt 0 ]]; do
  case $1 in
    --windows) BUILD_WINDOWS=true ;;
    *) echo "Unknown option: $1" ;;
  esac
  shift
done

# Set Unity path based on the platform
if [ "$BUILD_WINDOWS" = true ]; then
  UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"
  PACKAGE_DOWNLOAD_PATH="features/fixtures/mazerunner/Bugsnag.unitypackage"
else
  UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS/Unity"
  PACKAGE_DOWNLOAD_PATH="features/fixtures/mazerunner/Bugsnag.unitypackage"
fi

FIXTURE_PATH="features/fixtures/mazerunner"
DEFAULT_CLI_ARGS="-batchmode -nographics -quit"
BUGSNAG_RELEASE_URL="https://github.com/bugsnag/bugsnag-unity/releases/latest/download/Bugsnag.unitypackage"

# Download the latest Bugsnag Unity package if it doesn't exist or override it
echo "Downloading Bugsnag.unitypackage from $BUGSNAG_RELEASE_URL"
if ! curl -L --fail --silent --show-error "$BUGSNAG_RELEASE_URL" -o "$PACKAGE_DOWNLOAD_PATH"; then
  echo "Failed to download Bugsnag.unitypackage from $BUGSNAG_RELEASE_URL"
  exit 1
fi

# Verify the downloaded file
if [ ! -f "$PACKAGE_DOWNLOAD_PATH" ] || [ ! -s "$PACKAGE_DOWNLOAD_PATH" ]; then
  echo "Downloaded package is empty or missing: $PACKAGE_DOWNLOAD_PATH"
  exit 1
fi

echo "Successfully downloaded Bugsnag.unitypackage ($(stat -f%z "$PACKAGE_DOWNLOAD_PATH" 2>/dev/null || stat -c%s "$PACKAGE_DOWNLOAD_PATH") bytes)"

# Check if Unity path exists
if [ ! -f "$UNITY_PATH" ]; then
  echo "Unity executable not found at $UNITY_PATH"
  exit 1
fi

# Importing the Bugsnag package into Unity project
echo "Importing Bugsnag.unitypackage into $FIXTURE_PATH"
echo "Unity path: $UNITY_PATH"
echo "Package path: $PACKAGE_DOWNLOAD_PATH"

"$UNITY_PATH" $DEFAULT_CLI_ARGS \
            -projectPath $FIXTURE_PATH \
            -ignoreCompilerErrors \
            -importPackage "Bugsnag.unitypackage"
RESULT=$?
if [ $RESULT -ne 0 ]; then
  echo "Unity package import failed with exit code $RESULT"
  if [ -f "${FIXTURE_PATH}/import_package.log" ]; then
    echo "Last 50 lines of import log:"
    tail -50 "${FIXTURE_PATH}/import_package.log"
  fi
  exit $RESULT
fi

echo "Bugsnag package imported successfully"

# Proceed with unzipping the main package
root_path=$(pwd)
destination="features/fixtures/mazerunner/Packages"
package="$root_path/upm-package.zip"

rm -rf "$destination/package"
unzip -q "$package" -d "$destination"

echo "Package unzipped successfully"
