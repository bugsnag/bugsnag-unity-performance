#!/bin/bash -e
pushd features/fixtures/mazerunner/
  unzip mazerunner_macos_${UNITY_PERFORMANCE_VERSION:0:4}.zip
popd

rm -rf Gemfile.lock
bundle install
bundle exec maze-runner --app=features/fixtures/mazerunner/mazerunner_macos_dev_${UNITY_PERFORMANCE_VERSION:0:4}.app --os=macos --fail-fast features
