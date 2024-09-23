#!/usr/bin/env bash

# Import Bugsnag Unity package using the notifier script logic

FIXTURE_PATH="features/fixtures/mazerunner"

if [ -z "$UNITY_PERFORMANCE_VERSION" ]; then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" || "$OSTYPE" == "mingw"* ]]; then
  UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"
else
  UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS/Unity"
fi


DEFAULT_CLI_ARGS="-batchmode -nographics -quit"
BUGSNAG_RELEASE_URL="https://github.com/bugsnag/bugsnag-unity/releases/latest/download/Bugsnag.unitypackage"
PACKAGE_NAME="Bugsnag.unitypackage"
TEMP_DIR=$(mktemp -d)

# Download the latest Bugsnag Unity package
echo "Downloading Bugsnag.unitypackage from $BUGSNAG_RELEASE_URL"
curl -L "$BUGSNAG_RELEASE_URL" -o "$TEMP_DIR/$PACKAGE_NAME"
RESULT=$?
if [ $RESULT -ne 0 ]; then
  echo "Failed to download Bugsnag.unitypackage"
  exit $RESULT
fi

# Check if Unity path exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH"
  exit 1
fi

# Importing the Bugsnag package into Unity project
echo "Importing Bugsnag.unitypackage into $FIXTURE_PATH"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $FIXTURE_PATH \
                  -ignoreCompilerErrors \
                  -importPackage "$TEMP_DIR/$PACKAGE_NAME"
RESULT=$?
if [ $RESULT -ne 0 ]; then
  echo "Failed to import Bugsnag.unitypackage"
  exit $RESULT
fi

# Cleanup
echo "Cleaning up temporary files"
rm -rf "$TEMP_DIR"

echo "Bugsnag package imported successfully"

# Proceed with unzipping the main package
root_path=`pwd`
destination="features/fixtures/mazerunner/Packages"
package="$root_path/upm-package.zip"

rm -rf "$destination/package"
unzip -q "$package" -d "$destination"

echo "Package unzipped successfully"
