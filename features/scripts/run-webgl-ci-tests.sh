#!/bin/bash -e

pushd features/fixtures/mazerunner
  unzip mazerunner_$UNITY_PERFORMANCE_VERSION.zip
popd

bundle install

bundle exec maze-runner --farm=local --browser=firefox -e features