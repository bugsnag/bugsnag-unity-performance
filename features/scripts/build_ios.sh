#!/usr/bin/env bash

set -euo pipefail
IFS=$'\n\t'

# === CONFIGURATION ===
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly FIXTURES_DIR="$(cd "$SCRIPT_DIR/../fixtures" && pwd)"
readonly PROJECT_ROOT="$FIXTURES_DIR/mazerunner"
readonly ARCHIVE_PATH="$PROJECT_ROOT/archive/Unity-iPhone.xcarchive"
readonly OUTPUT_DIR="$PROJECT_ROOT/output"

# === USAGE CHECK ===
if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <dev|release>"
  exit 1
fi

readonly BUILD_TYPE="$1"

if [[ "$BUILD_TYPE" != "dev" && "$BUILD_TYPE" != "release" ]]; then
  echo "Error: Invalid build type '$BUILD_TYPE'. Use 'dev' or 'release'."
  exit 1
fi

# === SELECT PROJECT DIR AND IPA NAME ===
if [[ "$BUILD_TYPE" == "dev" ]]; then
  readonly PROJECT_DIR="$PROJECT_ROOT/mazerunner_dev_xcode"
  readonly IPA_NAME="mazerunner_dev"
else
  readonly PROJECT_DIR="$PROJECT_ROOT/mazerunner_xcode"
  readonly IPA_NAME="mazerunner"
fi

# === CLEAN OLD BUILDS ===
echo "Cleaning previous builds..."
find "$OUTPUT_DIR" -name "*.ipa" -exec rm -f {} +

# === ARCHIVE PROJECT ===
echo "Archiving project..."
xcrun xcodebuild -project "$PROJECT_DIR/Unity-iPhone.xcodeproj" \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath "$ARCHIVE_PATH" \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
                 GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
                 archive || {
                   echo "❌ Failed to archive project"
                   exit 1
                 }

# === EXPORT ARCHIVE ===
echo "Exporting archive..."
xcrun xcodebuild -exportArchive \
                 -archivePath "$ARCHIVE_PATH" \
                 -exportPath "$OUTPUT_DIR" \
                 -exportOptionsPlist "$SCRIPT_DIR/exportOptions.plist" \
                 -quiet || {
                   echo "❌ Failed to export app"
                   exit 1
                 }

# === RENAME IPA ===
IPA_PATH="$(find "$OUTPUT_DIR" -name "*.ipa" | head -n 1)"
if [[ -z "$IPA_PATH" ]]; then
  echo "❌ IPA not found after export"
  exit 1
fi

readonly FINAL_IPA="${PROJECT_ROOT}/${IPA_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.ipa"
mv "$IPA_PATH" "$FINAL_IPA"

echo "✅ Build succeeded: $FINAL_IPA"
