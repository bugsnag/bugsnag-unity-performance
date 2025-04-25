#!/usr/bin/env bash

set -e

if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

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

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_webgl.log"

project_path=`pwd`/mazerunner

# Determine the execute method and output folder based on build type
if [ "$BUILD_TYPE" == "dev" ]; then
  EXECUTE_METHOD="Builder.WebGLDev"
  OUTPUT_FOLDER="mazerunner_webgl_dev"
else
  EXECUTE_METHOD="Builder.WebGLRelease"
  OUTPUT_FOLDER="mazerunner_webgl"
fi

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi

mv $project_path/$OUTPUT_FOLDER $project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}

(cd $project_path && zip -q -r ${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}.zip ${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4})

popd
