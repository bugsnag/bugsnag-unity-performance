# Changelog

## v1.9.0 (2025-05-27)

### Additions

- Add FPS, CPU and Memory metrics to custom spans. [#183](https://github.com/bugsnag/bugsnag-unity-performance/pull/183)
- Set default endpoints based on API key [#184](https://github.com/bugsnag/bugsnag-unity-performance/pull/184)

### Deprecations

- SpanOptions.InstrumentRendering has been deprecated in favor of the new SpanOptions.SpanMetrics option and will be removed in a future release. [#172](https://github.com/bugsnag/bugsnag-unity-performance/pull/172)

## v1.8.1 (2025-03-27)

### Bug Fixes

- Fix issue where scripting symbols were not always correctly set in batch mode. [#164](https://github.com/bugsnag/bugsnag-unity-performance/pull/164)

## v1.8.0 (2025-02-05)

### Additions

- Add frame rate metrics to spans. [#155](https://github.com/bugsnag/bugsnag-unity-performance/pull/155)

### Bug Fixes

- Fix an issue where spans started with null names sent no name field. Now string.Empty will be sent instead [#154](https://github.com/bugsnag/bugsnag-unity-performance/pull/154)

## v1.7.1 (2024-12-06)

### Bug Fixes

- Fix an issue where using this SDK with v8.3.0+ of the BugSnag Unity Notifier would cause an exception while trying to resolve the BugsnagUnityWebRequest wrapper [#151](https://github.com/bugsnag/bugsnag-unity-performance/pull/151)

## v1.7.0 (2024-11-14)

### Additions

- Add support for Unity V6. [#148](https://github.com/bugsnag/bugsnag-unity-performance/pull/148)

### Dependencies

- Updated BugsnagUnityWebRequest to remove deprecation warnings and add new PostWwwForm methods introduced in Unity 2022.2 [#148](https://github.com/bugsnag/bugsnag-unity-performance/pull/148)

## v1.6.2 (2024-10-30)

### Bug Fixes

- Fix an issue where DateTime.Now was used instead of DateTimeOffset.UtcNow during initialisation [#146](https://github.com/bugsnag/bugsnag-unity-performance/pull/146)

## v1.6.1 (2024-10-09)

### Bug Fixes

- Fix an issue where config.Endpoiunt was incorrectly used in the delivery class [#143](https://github.com/bugsnag/bugsnag-unity-performance/pull/143)

- Fix an issue where Span.droppedAttributeCount was incorrectly implemented. [#140](https://github.com/bugsnag/bugsnag-unity-performance/pull/140)

## v1.6.0 (2024-09-24)

### Additions

- Add configurable limits to custom attributes. [#137](https://github.com/bugsnag/bugsnag-unity-performance/pull/137)

- Add TracePropagationUrls to the configuration window. [#136](https://github.com/bugsnag/bugsnag-unity-performance/pull/136)

## v1.5.1 (2024-09-09)

### Bug Fixes

- Fix an issue where the access to the finished span queue in the tracer was not thread safe. [#132](https://github.com/bugsnag/bugsnag-unity-performance/pull/132)

## v1.5.0 (2024-09-03)

### Additions

- Allow setting a fixed Span Sampling probability. [#128](https://github.com/bugsnag/bugsnag-unity-performance/pull/128)

- Allow setting custom span attributes. [#124](https://github.com/bugsnag/bugsnag-unity-performance/pull/124)

- Changed internal Span references to WeakReferences to avoid memory leaks. [#127](https://github.com/bugsnag/bugsnag-unity-performance/pull/127)

- Use API key subdomain as default Performance endpoint. [#129](https://github.com/bugsnag/bugsnag-unity-performance/pull/129)

- Added the service name resource attribute. [#130] (https://github.com/bugsnag/bugsnag-unity-performance/pull/130)

### Bug Fixes

- Fix an issue where TracePropagationUrls was incorrectly named and typed. [#126](https://github.com/bugsnag/bugsnag-unity-performance/pull/126)

## v1.4.2 (2024-06-27)

### Bug Fixes

- Fix an issue where spans with MakeCurrentContext set to false and passed as a parent would be added to the context stack. [#122](https://github.com/bugsnag/bugsnag-unity-performance/pull/122)

## v1.4.1 (2024-06-11)

### Bug Fixes

- Fixed issue where creating a span in a background thread caused an exception (mono or IL2CPP Dev builds only) [#117](https://github.com/bugsnag/bugsnag-unity-performance/pull/117)

## v1.4.0 (2024-05-30)

### Additions

- Added error correlation functionality so that when a compatible version of the [Bugsnag Unity Error Notifier](https://github.com/bugsnag/bugsnag-unity) is used, it can get the current SpanContext and attach it to error reports. [#112](https://github.com/bugsnag/bugsnag-unity-performance/pull/112)

- Added the trace parent header to requests made via the Bugsnag request wrapper and allow configuration via new TracePropagationUrls property. [#109](https://github.com/bugsnag/bugsnag-unity-performance/pull/109)

- Added methods to generate anonymous id value [#92](https://github.com/bugsnag/bugsnag-unity-performance/pull/92)

### Bug Fixes

- Fixed issue where the net.host.connection.type span attribute was not present in all spans [#114](https://github.com/bugsnag/bugsnag-unity-performance/pull/114)

## v1.3.4 (2024-04-29)

### Additions

- Discard open spans when app is backgrounded [#105](https://github.com/bugsnag/bugsnag-unity-performance/pull/105)

### Bug Fixes

- Fixed issue where spans could not be ended with a custom end time. [#106](https://github.com/bugsnag/bugsnag-unity-performance/pull/106)

- Fixed issue where custom spans with the SpanOption IsFirstClass set to false still had it reported as true. [#107](https://github.com/bugsnag/bugsnag-unity-performance/pull/107)

## v1.3.3 (2024-03-07)

### Bug Fixes

- Fixed issue where the p value header was being parsed without specific formatting instructions, meaning that when running in different locals, it could be parsed incorrectly. [#101](https://github.com/bugsnag/bugsnag-unity-performance/pull/101)

## v1.3.2 (2024-14-02)

### Bug Fixes

- Fixed issue where the span attribute android_api_version was sent as a int attribute instead of a string [#99](https://github.com/bugsnag/bugsnag-unity-performance/pull/99)

## v1.3.1 (2024-31-01)

### Additions

- Added Apple privacy manifest due to IO api usage. Please see the [Unity documentation](https://docs.unity3d.com/2023.3/Documentation/Manual/apple-privacy-manifest-policy.html#CSharpDotNetAPIs) for more information. [#94](https://github.com/bugsnag/bugsnag-unity-performance/pull/94)

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
