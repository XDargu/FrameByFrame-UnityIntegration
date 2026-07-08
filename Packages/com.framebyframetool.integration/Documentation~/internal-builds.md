# Internal Builds

Frame by Frame can run in pure Unity player builds for internal debugging. It should not be enabled in public release builds.

`[IMAGE PLACEHOLDER: Build Settings window with Development Build checked.]`

## Build Startup

In non-editor builds, `FbFManager` has a runtime initializer. Before scene load, it:

1. Loads `Resources/FrameByFrameRuntimeSettings`.
2. Applies port, protocol, timing, and recording option defaults.
3. Checks whether builds are enabled.
4. Checks whether the current build is a development build if required.
5. Starts the websocket server.

## Runtime Settings Asset

Save **Project Settings > Frame by Frame** to generate:

```text
Assets/FrameByFrame/Resources/FrameByFrameRuntimeSettings.asset
```

This asset is included in builds through Unity's `Resources` system.

## Recommended Settings

For most teams:

- enable **Enable In Builds**
- enable **Development Builds Only**
- use a known internal port
- keep all expensive recording options disabled by default
- enable options from the Frame by Frame viewer during a debugging session

## Release Builds

Do not ship public builds with the Frame by Frame server enabled.

Recommended safeguards:

- keep **Development Builds Only** enabled
- use CI checks to prevent release builds with debug settings
- avoid enabling expensive options by default
- restrict internal builds to trusted networks

## Manual Runtime Control

If your project needs explicit startup logic, disable automatic build startup and call:

```csharp
FbFManager.StartServer();
```

You can also change options at runtime:

```csharp
FbFManager.SetRecordingOption("Navigation", true);
FbFManager.SetRecordingOption("Colliders", false);
```
