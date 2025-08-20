#!/usr/bin/env bash
set -e  

if [ -z "$UNITY_PERFORMANCE_VERSION" ]; then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"
DEFAULT_CLI_ARGS="-quit -batchmode -nographics"
PROJECT_PATH="BugsnagPerformance"
SOLUTION_PATH="$PROJECT_PATH/BugsnagPerformance.sln"

# Check if Unity executable exists
if [ ! -f "$UNITY_PATH/Unity" ]; then
  echo "Unity executable not found at $UNITY_PATH/Unity"
  exit 1
fi

# Check if project path exists
if [ ! -d "$PROJECT_PATH" ]; then
  echo "Project path not found: $PROJECT_PATH"
  exit 1
fi

echo "Generating .sln and project files..."
echo "Unity path: $UNITY_PATH/Unity"
echo "Project path: $PROJECT_PATH"

# Generate .sln and project files
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS -projectPath "$PROJECT_PATH" -executeMethod "UnityEditor.SyncVS.SyncSolution"
RESULT=$?
if [ $RESULT -ne 0 ]; then
  echo "Failed to generate solution files with exit code $RESULT"
  exit $RESULT
fi

# Check if solution file was generated
if [ ! -f "$SOLUTION_PATH" ]; then
  echo "Solution file not found after generation: $SOLUTION_PATH"
  exit 1
fi

echo "Checking if dotnet is available..."
if ! command -v dotnet &> /dev/null; then
  echo "dotnet command not found. Please install .NET SDK."
  exit 1
fi

# Decide which dotnet format command to run based on the argument
FORMAT_COMMAND="dotnet format"
if [ "$1" == "--verify" ]; then
  FORMAT_COMMAND="dotnet format --verify-no-changes"
  echo "Running code format verification..."
else
  echo "Running code format..."
fi

echo "Command: $FORMAT_COMMAND \"$SOLUTION_PATH\""

# Execute dotnet format
$FORMAT_COMMAND "$SOLUTION_PATH"
EXIT_CODE=$?

if [ "$EXIT_CODE" -ne 0 ]; then
  if [ "$1" == "--verify" ]; then
    echo "Error: Code formatting verification found issues. Run 'rake code:format' to fix them."
  else
    echo "Error: Code formatting failed."
  fi
  exit "$EXIT_CODE"
fi

echo "Code formatting completed successfully!"