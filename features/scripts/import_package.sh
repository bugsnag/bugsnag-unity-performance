#!/usr/bin/env bash



if [ -z "$UNITY_PERFORMANCE_VERSION" ]
then
  echo "UNITY_PERFORMANCE_VERSION must be set"
  exit 1
fi

UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_PERFORMANCE_VERSION/Unity.app/Contents/MacOS"

echo "\`Unity\` executable = $UNITY_PATH/Unity"

pushd "${0%/*}"
  script_path=`pwd`
popd

pushd "$script_path/../fixtures"

# Run unity and immediately exit afterwards, log all output, disable the
# package manager (we just don't need it and it slows things down)
DEFAULT_CLI_ARGS="-quit -batchmode -nographics -logFile import_package.log"
project_path=`pwd`/mazerunner

# Installing the Bugsnag package
echo "Importing BugsnagPerformance.unitypackage into $project_path"
$UNITY_PATH/Unity $DEFAULT_CLI_ARGS \
                  -projectPath $project_path \
                  -ignoreCompilerErrors \
                  -importPackage $script_path/../../BugsnagPerformance.unitypackage
RESULT=$?
if [ $RESULT -ne 0 ]; then exit $RESULT; fi
