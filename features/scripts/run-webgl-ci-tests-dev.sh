#!/bin/bash -e

pushd features/fixtures/mazerunner/
  unzip mazerunner_webgl_dev_${UNITY_PERFORMANCE_VERSION:0:4}.zip
popd

bundle install

bundle exec maze-runner --farm=local --browser=firefox --fail-fast features
