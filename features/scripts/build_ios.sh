#!/usr/bin/env bash

set -e

if [ -z "$1" ]
then
  echo "Build type must be specified (dev or release)"
  exit 1
fi

BUILD_TYPE=$1

if [ "$BUILD_TYPE" != "dev" ] && [ "$BUILD_TYPE" != "release" ]
then
  echo "Invalid build type specified. Use 'dev' or 'release'."
  exit 1
fi

pushd "${0%/*}"
  script_path=`pwd`
popd
pushd "$script_path/../fixtures"
project_path=`pwd`/mazerunner

# Clean any previous builds
find $project_path/output/ -name "*.ipa" -exec rm '{}' \;

# Determine project path based on build type
if [ "$BUILD_TYPE" == "dev" ]; then
  PROJECT_DIR="$project_path/mazerunner_dev_xcode"
  IPA_NAME="mazerunner_dev"
else
  PROJECT_DIR="$project_path/mazerunner_xcode"
  IPA_NAME="mazerunner"
fi

# Archive and export the project
xcrun xcodebuild -project $PROJECT_DIR/Unity-iPhone.xcodeproj \
                 -scheme Unity-iPhone \
                 -configuration Debug \
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
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
                 -archivePath $project_path/archive/Unity-iPhone.xcarchive \
                 -exportPath $project_path/output/ \
                 -quiet \
                 -exportOptionsPlist $script_path/exportOptions.plist

if [ $? -ne 0 ]; then
  echo "Failed to export app"
  exit 1
fi

# Move to known location for running (note - the name of the .ipa differs between Xcode versions)
find $project_path/output/ -name "*.ipa" -exec mv '{}' $project_path/${IPA_NAME}_${UNITY_PERFORMANCE_VERSION:0:4}.ipa \;

popd
