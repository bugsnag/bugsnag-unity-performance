#!/usr/bin/env bash

set -euo pipefail
IFS=$'\n\t'

# === VALIDATE INPUTS ===
if [[ $# -ne 2 ]]; then
  echo "Usage: $0 <Xcode project path> <export name>"
  exit 1
fi

readonly XCODE_PROJECT_PATH="$1"
readonly EXPORT_NAME="$2"

echo "📁 Xcode Project Path: $XCODE_PROJECT_PATH"
echo "📦 Export Name: $EXPORT_NAME"

readonly OUTPUT_DIR="$XCODE_PROJECT_PATH/output"
readonly ARCHIVE_PATH="$XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive"
readonly FINAL_IPA_PATH="features/fixtures/minimalapp/${EXPORT_NAME}.ipa"
readonly EXPORT_OPTIONS_PLIST="features/scripts/exportOptions.plist"

# === CLEAN PREVIOUS BUILDS ===
echo "🧹 Cleaning previous .ipa files..."
if [[ -d "$OUTPUT_DIR" ]]; then
  find "$OUTPUT_DIR" -name "*.ipa" -exec rm -f {} +
else
  echo "ℹ️ Output directory does not exist, skipping cleanup."
fi

# === ARCHIVE PROJECT ===
echo "📦 Archiving project..."
xcrun xcodebuild -project "$XCODE_PROJECT_PATH/Unity-iPhone.xcodeproj" \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath "$ARCHIVE_PATH" \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
                 GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
                 archive

echo "✅ Archive successful."

# === EXPORT ARCHIVE ===
echo "📤 Exporting archive..."
xcrun xcodebuild -exportArchive \
                 -archivePath "$ARCHIVE_PATH" \
                 -exportPath "$OUTPUT_DIR" \
                 -exportOptionsPlist "$EXPORT_OPTIONS_PLIST" \
                 -quiet

echo "✅ Export successful."

# === MOVE FINAL IPA ===
echo "🚚 Moving IPA file to final destination..."
IPA_FILE=$(find "$OUTPUT_DIR" -name "*.ipa" | head -n 1)

if [[ -z "$IPA_FILE" ]]; then
  echo "❌ Error: No IPA file found after export."
  exit 1
fi

mv -f "$IPA_FILE" "$FINAL_IPA_PATH"

echo "🎉 Build complete! IPA available at: $FINAL_IPA_PATH"
