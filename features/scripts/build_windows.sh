#!/bin/bash -e

set -m

DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile build_windows.log"

# Check if the argument is provided
if [ $# -ne 1 ]; then
  echo "Usage: $0 <release|dev>"
  exit 1
fi

# Set the mode based on the argument
RUN_MODE=$1

# Determine the project path in Windows style to pass to the Unity exe
SCRIPT_DIR=$(dirname "$(realpath $0)")
pushd $SCRIPT_DIR
  pushd ../..
    root_path=`pwd`
  popd
  pushd ../fixtures
    project_path="$root_path/features/fixtures/mazerunner"
    win_project_path=`wslpath -w "$project_path"`
    echo "WSL project path: $project_path"
    echo "Windows project path: $win_project_path"
  popd
popd

# remove post build job for iOS bitcode as namespace is not available and it doesn't seem possible to conditionally remove using preprocessor directives
rm "$project_path/Assets/Editor/DisablingBitcodeiOS.cs"
rm "$project_path/Assets/Editor/DisablingBitcodeiOS.cs.meta"

UNITY_PATH="/mnt/c/Program Files/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Editor/Unity.exe"

# Check if Unity path exists
if [ ! -f "$UNITY_PATH" ]; then
  echo "Unity executable not found at $UNITY_PATH"
  exit 1
fi

# Clean up old zip files
if [ -f "features/fixtures/mazerunner/build/Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip" ]; then
  echo "Removing old dev zip file"
  rm -f "features/fixtures/mazerunner/build/Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
fi

if [ -f "features/fixtures/mazerunner/build/Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip" ]; then
  echo "Removing old release zip file"
  rm -f "features/fixtures/mazerunner/build/Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
fi

# Clean up old Windows build directory
if [ -d "features/fixtures/mazerunner/build/Windows" ]; then
  echo "Removing old Windows build directory"
  rm -rf "features/fixtures/mazerunner/build/Windows"
fi

echo "Building Windows executable with Unity..."
echo "Unity path: $UNITY_PATH"
echo "Project path (WSL): $project_path"
echo "Project path (Windows): $win_project_path"
echo "Run mode: $RUN_MODE"

if [ "$RUN_MODE" == "dev" ]; then
  echo "Executing Unity build for dev..."
  "$UNITY_PATH" $DEFAULT_CLI_ARGS \
    -projectPath "$win_project_path" \
    -executeMethod "Builder.WindowsDev"
  ZIP_FILE="Windows-dev-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
elif [ "$RUN_MODE" == "release" ]; then
  echo "Executing Unity build for release..."
  "$UNITY_PATH" $DEFAULT_CLI_ARGS \
    -projectPath "$win_project_path" \
    -executeMethod "Builder.WindowsRelease"
  ZIP_FILE="Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip"
else
  echo "Invalid argument: $RUN_MODE. Use 'release' or 'dev'."
  exit 1
fi

RESULT=$?
if [ $RESULT -ne 0 ]; then 
  echo "Unity build failed with exit code $RESULT"
  if [ -f "$project_path/build_windows.log" ]; then
    echo "Last 50 lines of build log:"
    tail -50 "$project_path/build_windows.log"
  fi
  exit $RESULT
fi

# Verify Windows build directory exists
if [ ! -d "features/fixtures/mazerunner/build/Windows" ]; then
  echo "Error: Expected Windows build directory not found"
  exit 1
fi

# Zip up the built artifacts
echo "Creating zip archive: $ZIP_FILE"
cd features/fixtures/mazerunner/build
if ! zip -r $ZIP_FILE Windows; then
  echo "Error: Failed to create zip archive"
  exit 1
fi

echo "Build completed successfully!"
echo "Output: features/fixtures/mazerunner/build/$ZIP_FILE"
