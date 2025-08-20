#!/usr/bin/env bash

set -e

if [ -z "$1" ]; then
  echo "Build type must be specified (dev or release)"
  exit 1
fi

BUILD_TYPE=$1

if [ "$BUILD_TYPE" != "dev" ] && [ "$BUILD_TYPE" != "release" ]; then
  echo "Invalid build type specified. Use 'dev' or 'release'."
  exit 1
fi

if [ -z "$UNITY_PERFORMANCE_VERSION" ]; then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

pushd "${0%/*}"
  script_path=`pwd`
popd
pushd "$script_path/../fixtures"
project_path=`pwd`/mazerunner

# Determine project path based on build type
if [ "$BUILD_TYPE" == "dev" ]; then
  PROJECT_DIR="$project_path/mazerunner_dev_xcode"
  IPA_NAME="mazerunner_dev"
else
  PROJECT_DIR="$project_path/mazerunner_xcode"
  IPA_NAME="mazerunner"
fi

FINAL_IPA_NAME="${IPA_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.ipa"

# Validate required files exist
if [ ! -d "$PROJECT_DIR" ]; then
  echo "ERROR: Xcode project directory not found: $PROJECT_DIR"
  exit 1
fi

if [ ! -d "$PROJECT_DIR/Unity-iPhone.xcodeproj" ]; then
  echo "ERROR: Xcode project not found: $PROJECT_DIR/Unity-iPhone.xcodeproj"
  exit 1
fi

if [ ! -f "$script_path/exportOptions.plist" ]; then
  echo "ERROR: Export options plist not found: $script_path/exportOptions.plist"
  exit 1
fi

# Clean any previous builds
echo "🧹 Cleaning previous build artifacts..."
if [[ -d "$project_path/output/" ]]; then
  echo "Removing old output directory..."
  rm -rf "$project_path/output/"
fi

if [[ -d "$project_path/archive/" ]]; then
  echo "Removing old archive directory..."
  rm -rf "$project_path/archive/"
fi

# Remove old IPA if it exists
if [ -f "$project_path/$FINAL_IPA_NAME" ]; then
  echo "Removing old IPA: $FINAL_IPA_NAME"
  rm -f "$project_path/$FINAL_IPA_NAME"
fi

# Create directories
mkdir -p "$project_path/output/"
mkdir -p "$project_path/archive/"

echo "📦 Starting iOS build process for $BUILD_TYPE..."
echo "Project: $PROJECT_DIR"
echo "Final IPA: $FINAL_IPA_NAME"

# Archive and export the project
xcrun xcodebuild -project "$PROJECT_DIR/Unity-iPhone.xcodeproj" \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath "$project_path/archive/Unity-iPhone.xcarchive" \
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
if [ ! -d "$project_path/archive/Unity-iPhone.xcarchive" ]; then
  echo "❌ Archive was not created at expected location"
  exit 1
fi

echo "✅ Archive completed successfully"
echo "📱 Starting export process..."

xcrun xcodebuild -exportArchive \
                 -archivePath "$project_path/archive/Unity-iPhone.xcarchive" \
                 -exportPath "$project_path/output/" \
                 -quiet \
                 -exportOptionsPlist "$script_path/exportOptions.plist"

EXPORT_RESULT=$?
if [ $EXPORT_RESULT -ne 0 ]; then
  echo "❌ Failed to export app with exit code $EXPORT_RESULT"
  exit 1
fi

echo "✅ Export completed successfully"
echo "📁 Moving IPA to final location..."

# Move to known location for running (note - the name of the .ipa differs between Xcode versions)
IPA_COUNT=$(find "$project_path/output/" -name "*.ipa" | wc -l)
if [ $IPA_COUNT -eq 0 ]; then
  echo "❌ No IPA files found in output directory"
  ls -la "$project_path/output/"
  exit 1
fi

if [ $IPA_COUNT -gt 1 ]; then
  echo "⚠️ Multiple IPA files found, using the first one:"
  find "$project_path/output/" -name "*.ipa" -ls
fi

# Move the IPA file to final location
find "$project_path/output/" -name "*.ipa" -exec mv '{}' "$project_path/$FINAL_IPA_NAME" \; -quit

# Verify the final IPA exists
if [ ! -f "$project_path/$FINAL_IPA_NAME" ]; then
  echo "❌ Failed to move IPA to final location"
  exit 1
fi

echo "✅ IPA successfully created: $FINAL_IPA_NAME"
ls -lh "$project_path/$FINAL_IPA_NAME"

popd
