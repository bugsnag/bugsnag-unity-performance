#!/bin/bash

SRC_DIR=BugsnagPerformance/Assets/BugsnagPerformance
PKG_DIR=upm/package

# Check for unity version
if [ -z "$1" ]
then
  echo "ERROR: No Version Set, please pass a version string as the first argument"
  exit 1
fi

VERSION=$1

echo "Creating package structure"

rm -rf "$PKG_DIR"

mkdir -p "$PKG_DIR"
mkdir -p "$PKG_DIR/Runtime"

cp -a  "$SRC_DIR/Plugins" "$PKG_DIR/Runtime"
cp -a  "$SRC_DIR/Plugins.meta" "$PKG_DIR/Runtime"

cp -a  "$SRC_DIR/Scripts" "$PKG_DIR/Runtime"
cp -a  "$SRC_DIR/Scripts.meta" "$PKG_DIR/Runtime"

cp -a  "$SRC_DIR/Editor" "$PKG_DIR"
cp -a  "$SRC_DIR/Editor.meta" "$PKG_DIR"

cp -a "upm/upm-resources/." "$PKG_DIR"


echo "Setting the version $VERSION in the copied manifest and readme"

sed -i '' "s/VERSION_STRING/$VERSION/g" "$PKG_DIR/package.json"
sed -i '' "s/VERSION_STRING/v$VERSION/g" "$PKG_DIR/README.md"

cd upm

zip -q -r "upm-package.zip" "package"

cd ..

mv "upm/upm-package.zip" .

echo "complete, ready to deploy"
