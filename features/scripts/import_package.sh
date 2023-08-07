#!/usr/bin/env bash

root_path=`pwd`

destination="features/fixtures/mazerunner/Packages"

package="$root_path/upm-package.zip"

rm -rf "$destination/package"

unzip "$package" -d "$destination"
