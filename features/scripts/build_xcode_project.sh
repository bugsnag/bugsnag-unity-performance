#!/usr/bin/env bash

set -e

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Path Set"
  exit 1
fi

XCODE_PROJECT_PATH=$1

echo "Xcode Project path set to $XCODE_PROJECT_PATH"

# Check for unity version
if [ -z "$2" ]
then
  echo "ERROR: No export name Set"
  exit 1
fi

EXPORT_NAME=$2


echo "Xcode export name set to $EXPORT_NAME"

# Clean any previous builds

# === CLEAN PREVIOUS BUILDS ===
echo "🧹 Cleaning previous .ipa files..."
if [[ -d "$XCODE_PROJECT_PATH/output/" ]]; then
  find $XCODE_PROJECT_PATH/output/ -name "*.ipa" -exec rm '{}' \;
else
  echo "ℹ️ Output directory does not exist, skipping cleanup."
fi

# Archive and export the project
xcrun xcodebuild -project $XCODE_PROJECT_PATH/Unity-iPhone.xcodeproj \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath $XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
                 GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
                 archive

if [ $? -ne 0 ]
then
  echo "Failed to archive project"
  exit 1
fi

xcrun xcodebuild -exportArchive \
                 -archivePath $XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive \
                 -exportPath $XCODE_PROJECT_PATH/output/ \
                 -quiet \
                 -exportOptionsPlist features/scripts/exportOptions.plist

if [ $? -ne 0 ]; then
  echo "Failed to export app"
  exit 1
fi

# === MOVE FINAL IPA ===
echo "🚚 Moving IPA file to final destination..."
IPA_FILE=$(find "$OUTPUT_DIR" -name "*.ipa" | head -n 1)

if [[ -z "$IPA_FILE" ]]; then
  echo "❌ Error: No IPA file found after export."
  exit 1
fi

mv -f "$IPA_FILE" "$FINAL_IPA_PATH"

echo "🎉 Build complete! IPA available at: $FINAL_IPA_PATH"
