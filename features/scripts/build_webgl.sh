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
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_webgl.log"

project_path=`pwd`/mazerunner

# Build for Android

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod Builder.WebGL
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/mazerunner_webgl $project_path/mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}

(cd $project_path && zip -q -r mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}.zip mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4})
