#!/bin/bash -e
pushd features/fixtures/mazerunner/
  unzip mazerunner_windows_$UNITY_PERFORMANCE_VERSION.zip
popd

bundle install
bundle exec maze-runner --app=features/fixtures/mazerunner/mazerunner_windows_$UNITY_PERFORMANCE_VERSION.app --os=windows features 
