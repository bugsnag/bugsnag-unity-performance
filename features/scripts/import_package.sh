#!/usr/bin/env bash

root_path=`pwd`

destination="features/fixtures/mazerunner/Packages"

package="$root_path/upm/package"

rm -rf "$destination/package"

cp -r  "$package" "$destination"
