#!/usr/bin/env sh

if [[ "$BUILDKITE_MESSAGE" == *"[full ci]"* ||
  "$BUILDKITE_BRANCH" == "next" ||
  "$BUILDKITE_BRANCH" == "main" ||
  "$BUILDKITE_BRANCH" == releases/* ||
  ! -z "$FULL_SCHEDULED_BUILD" ||
  "$BUILDKITE_PULL_REQUEST_BASE_BRANCH" == "main" ]]; then
  echo "Running full build"
  buildkite-agent pipeline upload .buildkite/pipeline.full.yml
else
  # Basic build, but allow a full build to be triggered
  echo "Running basic build"
  buildkite-agent pipeline upload .buildkite/block.yml
fi
