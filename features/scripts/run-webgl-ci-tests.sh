#!/bin/bash -e

pushd features/fixtures/mazerunner/
  unzip mazerunner_webgl_${UNITY_PERFORMANCE_VERSION:0:4}.zip
popd

bundle install

bundle exec maze-runner --farm=local --browser=firefox features