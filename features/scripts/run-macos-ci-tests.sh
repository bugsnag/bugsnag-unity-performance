#!/bin/bash -e
pushd features/fixtures/mazerunner/
  unzip mazerunner_$UNITY_PERFORMANCE_VERSION.zip
popd

bundle install
bundle exec maze-runner --fail-fast --app=features/fixtures/mazerunner/mazerunner_$UNITY_PERFORMANCE_VERSION.app --os=macos features 
