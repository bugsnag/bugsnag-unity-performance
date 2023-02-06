#!/usr/bin/env bash

if [ -z "$UNITY_PATH" ]
then
  echo "UNITY_PATH must be set, to e.g. /Applications/Unity/Hub/Editor/2018.4.36f1/Unity.app/Contents/MacOS"
  exit 1
fi

echo "\`which Unity\`=`which Unity`"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

DEFAULT_CLI_ARGS="-quit -nographics -batchmode -logFile generateXcodeProject.log"

project_path=`pwd`/mazerunner

# Generate the Xcode project for iOS
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.IosBuild
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

