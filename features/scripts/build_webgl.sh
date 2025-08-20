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

# Check if Unity path exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

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

# Clean up old build artifacts
old_folder_path="$project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}"
old_zip_path="$project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}.zip"

echo "Cleaning up old build artifacts..."
if [ -d "$old_folder_path" ]; then
  echo "Removing old folder: $old_folder_path"
  rm -rf "$old_folder_path"
fi

if [ -f "$old_zip_path" ]; then
  echo "Removing old zip: $old_zip_path"
  rm -f "$old_zip_path"
fi

# Also clean up the immediate build output
if [ -d "$project_path/$OUTPUT_FOLDER" ]; then
  echo "Removing previous build output: $project_path/$OUTPUT_FOLDER"
  rm -rf "$project_path/$OUTPUT_FOLDER"
fi

echo "Building WebGL with Unity..."
echo "Unity path: $UNITY_PATH/Unity"
echo "Project path: $project_path"
echo "Execute method: $EXECUTE_METHOD"

$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath $project_path -executeMethod $EXECUTE_METHOD
RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Unity build failed with exit code $RESULT"
  if [ -f "$project_path/build_webgl.log" ]; then
    echo "Last 50 lines of build log:"
    tail -50 "$project_path/build_webgl.log"
  fi
  exit $RESULT
fi

# Check if the build output exists
if [ ! -d "$project_path/$OUTPUT_FOLDER" ]; then
  echo "Error: Expected build output not found: $project_path/$OUTPUT_FOLDER"
  exit 1
fi

echo "Renaming build output..."
mv $project_path/$OUTPUT_FOLDER $project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}

echo "Creating zip archive..."
(cd $project_path && zip -q -r ${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}.zip ${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4})

# Verify the zip was created successfully
if [ ! -f "$project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}.zip" ]; then
  echo "Error: Failed to create zip archive"
  exit 1
fi

echo "Build completed successfully!"
echo "Output: $project_path/${OUTPUT_FOLDER}_${UNITY_PERFORMANCE_VERSION:0:4}.zip"

popd
