#!/bin/bash -e
pushd features/fixtures/mazerunner/build
  unzip Windows-${UNITY_PERFORMANCE_VERSION:0:4}.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/mazerunner/build/Windows/mazerunner_windows.exe --os=windows --fail-fast features
