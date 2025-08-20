#!/usr/bin/env bash

set -e

# Check for xcode project path
if [ -z "$1" ]; then
  echo "ERROR: No Xcode project path specified"
  echo "Usage: $0 <xcode_project_path> <export_name>"
  exit 1
fi

XCODE_PROJECT_PATH=$1

echo "Xcode Project path set to $XCODE_PROJECT_PATH"

# Validate xcode project path exists
if [ ! -d "$XCODE_PROJECT_PATH" ]; then
  echo "ERROR: Xcode project path does not exist: $XCODE_PROJECT_PATH"
  exit 1
fi

# Check for export name
if [ -z "$2" ]; then
  echo "ERROR: No export name specified"
  echo "Usage: $0 <xcode_project_path> <export_name>"
  exit 1
fi

EXPORT_NAME=$2

echo "Xcode export name set to $EXPORT_NAME"

# Validate required files exist
XCODE_PROJECT="$XCODE_PROJECT_PATH/Unity-iPhone.xcodeproj"
if [ ! -d "$XCODE_PROJECT" ]; then
  echo "ERROR: Xcode project not found: $XCODE_PROJECT"
  exit 1
fi

EXPORT_OPTIONS="features/scripts/exportOptions.plist"
if [ ! -f "$EXPORT_OPTIONS" ]; then
  echo "ERROR: Export options plist not found: $EXPORT_OPTIONS"
  exit 1
fi

# Clean any previous builds
echo "🧹 Cleaning previous build artifacts..."
if [[ -d "$XCODE_PROJECT_PATH/output/" ]]; then
  echo "Removing old output directory..."
  rm -rf "$XCODE_PROJECT_PATH/output/"
fi

if [[ -d "$XCODE_PROJECT_PATH/archive/" ]]; then
  echo "Removing old archive directory..."
  rm -rf "$XCODE_PROJECT_PATH/archive/"
fi

# Create output directories
mkdir -p "$XCODE_PROJECT_PATH/output/"
mkdir -p "$XCODE_PROJECT_PATH/archive/"

echo "📦 Starting Xcode archive process..."

# Archive and export the project
xcrun xcodebuild -project "$XCODE_PROJECT" \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath "$XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive" \
                 -allowProvisioningUpdates \
                 -allowProvisioningDeviceRegistration \
                 -quiet \
                 GCC_WARN_INHIBIT_ALL_WARNINGS=YES \
                 archive

ARCHIVE_RESULT=$?
if [ $ARCHIVE_RESULT -ne 0 ]; then
  echo "❌ Failed to archive project with exit code $ARCHIVE_RESULT"
  exit 1
fi

# Verify archive was created
if [ ! -d "$XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive" ]; then
  echo "❌ Archive was not created at expected location"
  exit 1
fi

echo "✅ Archive completed successfully"
echo "📱 Starting export process..."

xcrun xcodebuild -exportArchive \
                 -archivePath "$XCODE_PROJECT_PATH/archive/Unity-iPhone.xcarchive" \
                 -exportPath "$XCODE_PROJECT_PATH/output/" \
                 -quiet \
                 -exportOptionsPlist "$EXPORT_OPTIONS"

EXPORT_RESULT=$?
if [ $EXPORT_RESULT -ne 0 ]; then
  echo "❌ Failed to export app with exit code $EXPORT_RESULT"
  exit 1
fi

echo "✅ Export completed successfully"
echo "📁 Moving IPA to final location..."

# Move to known location for running (note - the name of the .ipa differs between Xcode versions)
IPA_COUNT=$(find "$XCODE_PROJECT_PATH/output/" -name "*.ipa" | wc -l)
if [ $IPA_COUNT -eq 0 ]; then
  echo "❌ No IPA files found in output directory"
  ls -la "$XCODE_PROJECT_PATH/output/"
  exit 1
fi

if [ $IPA_COUNT -gt 1 ]; then
  echo "⚠️ Multiple IPA files found, using the first one:"
  find "$XCODE_PROJECT_PATH/output/" -name "*.ipa" -ls
fi

# Ensure target directory exists
mkdir -p features/fixtures/minimalapp/

# Move the IPA file
find "$XCODE_PROJECT_PATH/output/" -name "*.ipa" -exec mv '{}' "features/fixtures/minimalapp/$EXPORT_NAME.ipa" \; -quit

# Verify the final IPA exists
if [ ! -f "features/fixtures/minimalapp/$EXPORT_NAME.ipa" ]; then
  echo "❌ Failed to move IPA to final location"
  exit 1
fi

echo "✅ IPA successfully created: features/fixtures/minimalapp/$EXPORT_NAME.ipa"
ls -lh "features/fixtures/minimalapp/$EXPORT_NAME.ipa"
