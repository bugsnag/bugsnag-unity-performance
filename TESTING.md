# Mazerunner tests

This document explains how to build the plugin, import it into the test fixture project, build platform-specific test apps, and run end-to-end Mazerunner tests using `maze-runner`.

---

## Prerequisites

- **Ruby** — required to run `rake` tasks and `bundle exec maze-runner`. Install gems with:
  ```sh
  bundle install
  ```
- **Unity Hub** — the Unity editor must be installed via Unity Hub. The editor is expected at:
  ```
  /Applications/Unity/Hub/Editor/<UNITY_PERFORMANCE_VERSION>/Unity.app
  ```
- **Android SDK** — required only for Android builds.
- **Xcode** — required only for iOS builds.

---

## Environment Variable

All build scripts and rake tasks use `UNITY_PERFORMANCE_VERSION` to locate the Unity editor. Set it once for your shell session before running any commands:

```sh
export UNITY_PERFORMANCE_VERSION=2021.3.58f1
```

You can also prefix it inline on individual commands (shown in the sections below).

Remote tests can be run against real devices provided by BrowserStack. In order to run these tests, you need to set 
the following environment variables:

- A BrowserStack App Automate Username: `BROWSER_STACK_USERNAME`
- A BrowserStack App Automate Access Key: `BROWSER_STACK_ACCESS_KEY`
- A path to a [BrowserStack local testing binary](https://www.browserstack.com/local-testing/app-automate): `MAZE_BS_LOCAL`

---

## Step 1 — Build the UPM Package

This step packages the plugin source from `BugsnagPerformance/Assets/BugsnagPerformance` into a UPM-compatible zip file (`upm-package.zip`) at the root of the repository. No Unity editor is required for this step.

```sh
rake plugin:build:export
```

The output file `upm-package.zip` will be created in the project root.

---

## Step 2 — Import the Package into the Test Fixture

The test fixture project lives at `features/fixtures/mazerunner`. The package must be imported before building any platform app.

### Option A — Using the import script (recommended)

This script downloads the latest `Bugsnag.unitypackage` from GitHub and imports it into the fixture project using Unity in batch mode. `UNITY_PERFORMANCE_VERSION` must be set.

```sh
./features/scripts/import_package.sh
```

### Option B — Manual extraction

If you already have `upm-package.zip` (built in Step 1) and want to skip the Unity import step, unzip it directly into the fixture's `Packages` directory:

```sh
mkdir -p features/fixtures/mazerunner/Packages
unzip -q upm-package.zip -d features/fixtures/mazerunner/Packages
```

---

## Step 3 — Build the Platform App

Choose the platform you want to test.

> **Important:** The `rake test:*` tasks automatically run `import_package.sh` as part of their build process. That script **downloads the latest released `Bugsnag.unitypackage` from GitHub** (not from your locally built `upm-package.zip`). If you want to test against your own local changes, skip the rake task and instead:
> 1. Complete **Step 2 — Option B** to extract your local package.
> 2. Run the platform build script directly (see the *"Using the local package"* notes in each section below).

### Android

Builds a dev APK at `features/fixtures/mazerunner/mazerunner-dev_<YEAR>.apk` (e.g. `mazerunner-dev_2021.apk`).

```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:android:build_dev
```

For a release APK:
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:android:build
```

### iOS

iOS building is a two-step process: first generate the Xcode project, then build the IPA. The IPA is placed at `features/fixtures/mazerunner/mazerunner_dev_<YEAR>.ipa`.

```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:ios:generate_xcode_dev
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:ios:build_xcode_dev
```

For a release IPA:
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:ios:generate_xcode
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:ios:build_xcode
```

### macOS

Builds a dev `.app` bundle at `features/fixtures/mazerunner/mazerunner_macos_dev_<YEAR>.app`.

```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:macos:build_dev
```

For a release build:
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:macos:build
```

### WebGL

Builds a dev WebGL app and zips it to `features/fixtures/mazerunner/mazerunner_webgl_dev_<YEAR>.zip`.

```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:webgl:build_dev
```

> **Note:** This rake task downloads the latest released `Bugsnag.unitypackage` from GitHub before building. Use this when you want to test against the latest published Bugsnag release.

For a release build:
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 rake test:webgl:build
```

### Windows
On Git Bash Terminal
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/import_package.sh --windows
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/build_windows.sh release
```

**Using the local package** — If you have already imported your local package via Step 2 (Option B) and want to skip the GitHub download, run the build script directly:
### Android
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/build_android.sh dev
```
### iOS
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/generate_xcode_project.sh dev
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/build_ios.sh dev
```
### macOS
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/build_macos.sh dev
```
### WebGL
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f1 features/scripts/build_webgl.sh dev
```
### Windows
On Git Bash Terminal
```sh
UNITY_PERFORMANCE_VERSION=2021.3.58f features/scripts/build_windows.sh dev

---

## Step 4 — Run Mazerunner Tests

`maze-runner` is the end-to-end test harness. Use `bundle exec maze-runner` to ensure the correct gem version is used. Replace `<YEAR>` with the first four characters of your `UNITY_PERFORMANCE_VERSION` (e.g. `2021` for `2021.3.58f1`).

### Android (remote — BrowserStack)

`--farm=bb` routes the test run through BrowserStack. `--device` specifies the BrowserStack device name.

```sh
bundle exec maze-runner \
  --app=./features/fixtures/mazerunner/mazerunner-dev_2021.apk \
  --farm=bb \
  --device=ANDROID_9
```

### iOS (remote — BrowserStack)

```sh
bundle exec maze-runner \
  --app=./features/fixtures/mazerunner/mazerunner-dev_2021.ipa \
  --farm=bb \
  --device=IOS_16
```

### macOS (local)

`--os=macos` tells maze-runner to launch the app locally as a macOS process.

```sh
bundle exec maze-runner \
  --app=features/fixtures/mazerunner/mazerunner_macos_dev_2021.app \
  --os=macos
```

### WebGL (local — browser)

WebGL tests are served by maze-runner's built-in HTTP server. The `--app` flag is **not** used. Before running, the WebGL zip produced by the build step must be extracted into `features/fixtures/mazerunner/`:

```sh
cd features/fixtures/mazerunner && unzip mazerunner_webgl_dev_2021.zip && cd -
```

Then run maze-runner without `--app`:

```sh
bundle exec maze-runner --farm=local --browser=firefox features/dev.feature
```

Replace `--browser=firefox` with `--browser=chrome` if you prefer Chrome. To run the full suite (excluding dev scenarios):

```sh
bundle exec maze-runner --farm=local --browser=firefox --exclude=features/dev.feature features
```

### windows (local)

```sh
bundle exec maze-runner --app=features/fixtures/mazerunner/mazerunner_windows_dev_2021.exe --os=windows
```
