# Changelog

## v1.3.1 (2024-08-01)

### Bug Fixes

- Fixed issue where http status code was reported as a string attribute instead of an int attribute [#93](https://github.com/bugsnag/bugsnag-unity-performance/pull/93)

## v1.3.0 (2023-10-05)

### Additions

- Added support for WebGL builds. [#77](https://github.com/bugsnag/bugsnag-unity-performance/pull/77)

- Added methods to enable manual creation of SceneLoad and Network spans. [#81](https://github.com/bugsnag/bugsnag-unity-performance/pull/81)

### Bug Fixes

- Fixed issue where the Android version code and iOS bundle version were incorrectly labelled [#76](https://github.com/bugsnag/bugsnag-unity-performance/pull/76)

- Fixed issue where some unity version required meta files within the macos bundle [#80](https://github.com/bugsnag/bugsnag-unity-performance/pull/80)


## v1.2.0 (2023-08-18)

### Additions

- Added support for MacOS builds. [#72](https://github.com/bugsnag/bugsnag-unity-performance/pull/72)

### Bug Fixes

- Fixed issue where the attribute os.version was incorrectly reported. [#72](https://github.com/bugsnag/bugsnag-unity-performance/pull/72)

## v1.1.0 (2023-08-03)

### Additions

- Allow Network request span customisation via callbacks. [#70](https://github.com/bugsnag/bugsnag-unity-performance/pull/70)

## v1.0.0 (2023-07-17)

- Official 1.0 release for GA

## v0.1.2 (2023-07-06)

### Bug Fixes

- Removed erroneous imports in scripts. [#63](https://github.com/bugsnag/bugsnag-unity-performance/pull/63)

- Fix typo in the example project. [#62](https://github.com/bugsnag/bugsnag-unity-performance/pull/62)


## v0.1.1 (2023-07-04)

### Bug Fixes

- Fix issue where empty EnabledReleaseStages resulted in the SDK not starting. [#60](https://github.com/bugsnag/bugsnag-unity-performance/pull/60)

- Fix issue where both BugSnag UPM packages had conflicting GUIDs. [#59](https://github.com/bugsnag/bugsnag-unity-performance/pull/59)
