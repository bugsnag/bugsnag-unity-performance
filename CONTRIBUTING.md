
# Contributing

## Releasing a new version

#### Making the package release

1. Make sure any changes made since last release in `main` are merged into `next`.

2. Checkout the `next` branch. Set the version number in the change log AND the `Version.cs` file.

3. Commit the changelog and version updates:

    ```
    git add CHANGELOG.md BugsnagPerformance/Assets/BugsnagPerformance/Scripts/Internal/Version.cs
    git commit -m "Release v1.x.x"
    ```
4. Make a pull request to merge the changes into `main`

5. Once merged, tag the new release version, pushing the tag to GitHub:

   ```
   git tag v1.x.x
   git push origin v6.x.x
   ```

6. Wait. The CI build will build the new package and create a draft release.

7. Verify that the release looks good, upload the unity packages to the release, copy in the changelog entry into the release notes and publish the draft.

#### Making the UPM release

Once the UnityPackage release is confirmed a UPM release should be deployed

1. Checkout the release commit on `main`

2. Build the upm package by running the `build-upm-package.sh` script in the `upm` directory. You should pass the version number of the release like so `./build-upm-package.sh 1.x.x`. You must run the script from within the `upm` directory. This will build the upm package in the `upm/package` directory.

3. Test that the built package installs by using the install local package option in unity package manager.

4. Clone the `bugsnag-unity-performance-upm` repo and make sure you are in the `main` branch.

5. Replace the contents of the repo with the contents of the `upm/package` directory in the `bugsnag-unity-performance` repo

6. Commit these changes to main with the message `Release V1.x.x`

7. Tag the release and push the tag
  ```
   git tag v1.x.x
   git push origin v1.x.x
   ```


