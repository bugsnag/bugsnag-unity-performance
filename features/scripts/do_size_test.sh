#!/usr/bin/env bash

set -euo pipefail
IFS=$'\n\t'

# === CHECK ENVIRONMENT ===
if [[ -z "${UNITY_PERFORMANCE_VERSION:-}" ]]; then
  echo "❌ UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

# === CONFIGURATION ===
readonly UNITY_PATH="/Applications/Unity/Hub/Editor/${UNITY_PERFORMANCE_VERSION}/Unity.app/Contents/MacOS"
readonly DEFAULT_CLI_ARGS="-quit -batchmode -nographics"

readonly PROJECT_PATH="features/fixtures/minimalapp"
readonly PACKAGE_PATH="upm-package.zip"
readonly PACKAGE_DESTINATION="${PROJECT_PATH}/Packages"

# === FUNCTIONS ===

run_unity_build() {
  local method=$1
  local log_file=$2

  echo "🏗️  Running Unity build: $method"
  "$UNITY_PATH/Unity" $DEFAULT_CLI_ARGS \
    -projectPath "$PROJECT_PATH" \
    -executeMethod "$method" \
    -logFile "$log_file"

  echo "✅ Finished: $method"
}

safe_source() {
  local script=$1
  if [[ -f "$script" ]]; then
    source "$script"
  else
    echo "⚠️  Warning: Script not found: $script"
    exit 1
  fi
}

# === CLEAN UP ===
echo "🧹 Removing existing packages..."
rm -rf "$PACKAGE_DESTINATION"

# === BUILD WITHOUT BUGSNAG ===
run_unity_build "Builder.BuildAndroidWithout" "build_android_minimal_without.log"
run_unity_build "Builder.BuildIosWithout" "export_ios_xcode_project_minimal_without.log"

safe_source "./features/scripts/build_xcode_project.sh" \
  "features/fixtures/minimalapp/minimal_without_xcode" \
  "without_bugsnag"

# === IMPORT PACKAGE ===
echo "📦 Importing package..."
unzip -q "$PACKAGE_PATH" -d "$PACKAGE_DESTINATION"

# === BUILD WITH BUGSNAG ===
run_unity_build "Builder.BuildAndroidWith" "build_android_minimal_with.log"
run_unity_build "Builder.BuildIosWith" "export_ios_xcode_project_minimal_with.log"

safe_source "./features/scripts/build_xcode_project.sh" \
  "features/fixtures/minimalapp/minimal_with_xcode" \
  "with_bugsnag"

# === DANGER CHECK ===
echo "🚦 Running Danger checks..."
(
  cd "$PROJECT_PATH"
  bundle install
  bundle exec danger
)

echo "🎉 All builds and checks completed successfully!"
