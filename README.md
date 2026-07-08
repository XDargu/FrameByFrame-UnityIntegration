# Frame by Frame for Unity

Unity integration for [Frame by Frame](https://www.framebyframetool.com), a visual debugging and recording tool for 3D applications.

![Frame by Frame for Unity example](Branding/ExampleScreen.png)

This repository contains the Unity Package Manager package:

```text
Packages/com.framebyframetool.integration
```

Package name:

```text
com.framebyframetool.integration
```

Version:

```text
1.0.0
```

Supported Frame by Frame viewer:

```text
Frame by Frame v1.1.0
```

## Requirements

- Unity 2022.3 or newer
- Frame by Frame v1.1.0 or newer
- `com.unity.nuget.newtonsoft-json`

The package manifest declares its Unity package dependencies.

## Installation

### Git URL

In Unity, open **Window > Package Manager**, click **+**, choose **Add package from git URL**, and enter:

```text
https://github.com/XDargu/FrameByFrame-UnityIntegration.git?path=/Packages/com.framebyframetool.integration#v1.0.0
```

### Embedded Package

For local development, keep the package embedded at:

```text
Packages/com.framebyframetool.integration
```

### Unity Asset Store

After Asset Store approval, install **Frame by Frame for Unity** from the Unity Asset Store and import the package into your project.

## Quick Start

1. Open **Window > Frame by Frame**.
2. Open **Edit > Project Settings > Frame by Frame** and choose the recording options you want enabled by default.
3. Start the Frame by Frame viewer.
4. Press **Start** in the Unity Frame by Frame window if the server is not already running.
5. Enter Play Mode.
6. Add recorder components such as `RecordPhysics`, `RecordAI`, `RecordAnimations`, or `RecordLog` to the GameObjects you want to inspect.

`[IMAGE PLACEHOLDER: Project Settings > Frame by Frame showing connection settings and recording option toggles.]`

## Editor Window

Open **Window > Frame by Frame** to control the local websocket server.

The window shows:

- server status
- connected client count
- websocket port and protocol
- play mode connection behavior
- last connection error
- raw recording controls
- discovered recording options

Recording options are discovered before Play Mode from `FrameByFrameRecordingOption` attributes, so teams can choose what to record before starting the game.

## Project Settings

Open **Edit > Project Settings > Frame by Frame**.

Connection settings:

- **Auto Start In Editor** starts the websocket server when the Unity editor loads.
- **Keep Alive Across Play Mode** keeps the server alive through play mode transitions.
- **Enable In Builds** allows the runtime initializer to start the server in player builds.
- **Development Builds Only** prevents the server from starting in non-development builds.
- **WebSocket Port** defaults to `23001`.
- **WebSocket Protocol** defaults to `frameByframe`.

Recording settings:

- **Raw Recording Path** sets the default `.fbf` raw recording path.
- **Recording Options** stores the default enabled state for each discovered option.

When settings are saved, the editor also writes a runtime settings asset to:

```text
Assets/FrameByFrame/Resources/FrameByFrameRuntimeSettings.asset
```

That asset lets internal player builds load the same build flags, connection settings, and recording option defaults.

## Built-in Recording Options

The package ships these built-in option IDs:

- `Animations`
- `Colliders`
- `Collisions`
- `Log`
- `Navigation`
- `NavMesh`
- `Physics`
- `ShapeHelpers`

Demo samples may add more options, such as `AI`, `Stats`, `Weapons`, and `Explosions`.

## Built-in Recorder Components

- `RecordPhysics`: records collider shapes, collision contacts, and Rigidbody velocity.
- `RecordAI`: records `NavMeshAgent` path and path status.
- `RecordNavMesh`: records the calculated NavMesh triangulation.
- `RecordAnimations`: records Animator layers, clips, playback state, and parameters.
- `RecordLog`: records Unity log messages and error events.
- `RecordPlane`: records a helper plane.
- `RecordScreenshot`: records screen captures as textured helper planes.

Add these components only where you want data recorded. For larger projects, prefer adding them through scene-specific recorder scopes or your own filtering logic so the plugin records relevant objects only.

## Custom Recording Options

Declare options with `FrameByFrameRecordingOption`.

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("Stats", "Records health, damage, and death events.")]
public class Stats : MonoBehaviour
{
	public int Health = 100;

	private void Update()
	{
		if (!FbFManager.IsRecordingOptionEnabled("Stats"))
		{
			return;
		}

		PropertyGroup props = FbFManager.RecordProperties(gameObject, "Stats");
		props.AddProperty("Health", Health, new Icon("heart"));
	}
}
```

Only `id` and `description` are required. The `id` is the stable value sent to the Frame by Frame viewer and used by code.

## Runtime API

Common calls:

```csharp
FbFManager.IsRecordingOptionEnabled("Physics");
FbFManager.SetRecordingOption("Physics", true);
FbFManager.RecordEntity(gameObject);
FbFManager.RecordProperties(gameObject, "Gameplay");
FbFManager.RecordEvent(gameObject, "Door Opened", "Gameplay");
FbFManager.RecordResource("debug-image.jpg", "image/jpg", jsonPayload);
```

Recording options can also be managed through:

```csharp
FbFRecordingOptions.Register("MyOption", "Records my custom data.");
FbFRecordingOptions.SetEnabled("MyOption", true);
FbFRecordingOptions.GetDefinitions();
```

## Internal Builds

Frame by Frame is intended for editor and internal/development builds, not public release builds.

By default, runtime startup is guarded by:

- `Config.enableInBuilds`
- `Config.developmentBuildsOnly`
- `Debug.isDebugBuild`

For development builds, save the Frame by Frame Project Settings so Unity generates the runtime settings asset under `Assets/FrameByFrame/Resources`.

## Documentation

Official package documentation lives under:

```text
Packages/com.framebyframetool.integration/Documentation~
```

Start with [Documentation~/README.md](Packages/com.framebyframetool.integration/Documentation~/README.md).

## License

Frame by Frame for Unity is distributed under the MIT License. See [LICENSE](LICENSE).
