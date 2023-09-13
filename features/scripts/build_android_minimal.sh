#!/usr/bin/env bash

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"


pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_android_minimal.log"

project_path=`pwd`/minimalapp


echo "remove existing packages"
rm -rf "$project_path/Packages"

# Build for Android
echo "building without bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWithout
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi


echo "import package"
cd ..
cd ..
unzip -q "upm-package.zip" -d "features/fixtures/minimalapp/Packages"

# Build for Android
echo "building with bugsnag"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.BuildAndroidWith
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

