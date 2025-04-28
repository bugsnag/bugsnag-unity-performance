#!/usr/bin/env bash

set -e

# === CHECK ENVIRONMENT ===
if [[ -z "${UNITY_PERFORMANCE_VERSION:-}" ]]; then
  echo "❌ UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

# === CONFIGURATION ===
UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_PERFORMANCE_VERSION}/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="features/fixtures/minimalapp"
PACKAGE_PATH="upm-package.zip"
PACKAGE_DESTINATION="${PROJECT_PATH}/Packages"

# === CLEAN UP ===
echo "🧹 Removing existing packages..."
rm -rf "$PACKAGE_DESTINATION"

echo "🏗️ building android without bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $PROJECT_PATH -executeMethod Builder.BuildAndroidWithout -logFile build_android_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "🏗️ building ios without bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $PROJECT_PATH -executeMethod Builder.BuildIosWithout -logFile export_ios_xcode_project_minimal_without.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_without_xcode without_bugsnag

echo "📦 Importing package..."
unzip -q "$PACKAGE_PATH" -d "$PACKAGE_DESTINATION"

echo "🏗️ building android with bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $PROJECT_PATH -executeMethod Builder.BuildAndroidWith -logFile build_android_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

echo "🏗️ building ios with bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $PROJECT_PATH -executeMethod Builder.BuildIosWith -logFile export_ios_xcode_project_minimal_with.log
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
ls

source ./features/scripts/build_xcode_project.sh features/fixtures/minimalapp/minimal_with_xcode with_bugsnag

# === DANGER CHECK ===
echo "🚦 Running Danger checks..."
(
  cd "$PROJECT_PATH"
  bundle install
  bundle exec danger
)

echo "🎉 All builds and checks completed successfully!"