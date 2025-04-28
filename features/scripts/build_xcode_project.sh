#!/usr/bin/env bash

set -euo pipefail
IFS=$'\n\t'

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Path Set"
  exit 1
fi

XCODE_PROJECT_PATH="$1"
EXPORT_NAME="$2"

echo "📁 Xcode Project Path: $XCODE_PROJECT_PATH"
echo "📦 Export Name: $EXPORT_NAME"

# Check for unity version
if [ -z "$2" ]
then
  echo "ERROR: No export name Set"
  exit 1
fi

# === CLEAN PREVIOUS BUILDS ===
echo "🧹 Cleaning previous .ipa files..."
if [[ -d "$XCODE_PROJECT_PATH/output/" ]]; then
  find $XCODE_PROJECT_PATH/output/ -name "*.ipa" -exec rm '{}' \;
else
  echo "ℹ️ Output directory does not exist, skipping cleanup."
fi

# Archive and export the project
echo "📦 Archiving project..."

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
else
  echo "✅ Archive successful."
fi

# === EXPORT ARCHIVE ===
echo "📤 Exporting archive..."

xcrun xcodebuild -exportArchive \
                 -archivePath $XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive \
                 -exportPath $XCODE_PROJECT_PATH/output/ \
                 -quiet \
                 -exportOptionsPlist features/scripts/exportOptions.plist

if [ $? -ne 0 ]; then
  echo "Failed to export app"
  exit 1
else
  echo "✅ Export successful."
fi

# === MOVE FINAL IPA ===
echo "🚚 Moving IPA file to final destination..."
IPA_FILE=$(find "$XCODE_PROJECT_PATH/output/" -name "*.ipa" | head -n 1)
FINAL_IPA_PATH="features/fixtures/minimalapp/$EXPORT_NAME.ipa"

if [[ -z "$IPA_FILE" ]]; then
  echo "❌ Error: No IPA file found after export."
  exit 1
fi

mv -f "$IPA_FILE" "$FINAL_IPA_PATH"

echo "🎉 Build complete! IPA available at: $FINAL_IPA_PATH"
