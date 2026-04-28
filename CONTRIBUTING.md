
# Contributing

## Releasing a new version

#### 1: Create release commit and tag

1. Make sure any changes made since last release in `main` are merged into `next`.

2. Checkout the `next` branch. Set the version number and releae date in CHANGELOG.md
3. Set the version number in the `BugsnagPerformance/Assets/BugsnagPerformance/Scripts/Internal/Version.cs` file.

4. Commit the changelog and version updates with the commit message `Release v1.x.x`

5. Make a pull request to merge the changes into `main`

6. Once merged, tag the merge commit with new release version, pushing the tag to GitHub:

   ```
   git tag v1.x.x
   git push origin v1.x.x
   ```

7. Create a release from the tag in github, copy the changelog entry into the release notes and publish the release.


#### 2: Making the UPM release

Once the github release is confirmed a UPM release should be deployed

1. Checkout the release commit on `main`

2. Go to the release merge commit build in build kite and download the upm package from the `Build Released Library Artifact` task.

3. Test that the built package installs by using the install local package option in unity package manager.

4. Clone the `bugsnag-unity-performance-upm` repo and make sure you are in the `main` branch and it is up to date with origin.

5. Replace the entire contents of the repo with the contents of the new package

6. Commit these changes to main with the message `Release v1.x.x`

7. Tag the release and push the tag
  ```
   git tag v1.x.x
   git push origin v1.x.x
   ```

## Known Issues

### MacOS Native Bundle Meta Files

This SDK contains a MacOS bundle as a native plugin. Some versions of Unity (There seems to be no pattern, we have seen different behaviour in different patch versions of 2019 2020 & 2021) import that bundle as a single file, others import it as a folder. 

When importing as a single file it creates only 1 meta file, but when importing as a folder, it creates meta files for every file and folder within.

This causes a problem because we distribute the SDK as a UPM package. When importing a UPM package, Unity cannot create meta files on the fly, so if we only distribute 1 meta file for the root of the bundle, assuming that it will be imported as a single file, and then Unity imports it as a folder, it will throw errors stating that it can’t find meta files for the contents of the directory and will not import the plugin.

To solve this, whenever the native code for MacOS is rebuilt (currently only when changes have been made) then the resulting bundle should be manually imported into unity 2019 and then copied over to the plugins folder of the dev project along with all relevant meta files.










