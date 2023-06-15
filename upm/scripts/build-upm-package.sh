#!/bin/bash

SRC_DIR=../../BugsnagPerformance/Assets/BugsnagPerformance/
PKG_DIR=../package

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1


echo "Copying over the src"

cp -a  "$SRC_DIR." "$PKG_DIR/Runtime"


# Set the specified version in the manifest and readme

echo "Setting the version $VERSION in the copied manifest and readme"
sed -i '' "s/VERSION_STRING/$VERSION/g" "$PKG_DIR/package.json"
sed -i '' "s/VERSION_STRING/v$VERSION/g" "$PKG_DIR/README.md"


echo "complete, ready to deploy"
