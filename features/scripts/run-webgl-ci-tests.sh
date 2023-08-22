#!/bin/bash -e
unzip mazerunner_webgl_$UNITY_PERFORMANCE_VERSION.zip

bundle install

bundle exec maze-runner --farm=local --browser=firefox features