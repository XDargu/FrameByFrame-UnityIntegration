# Quick Start

`[IMAGE PLACEHOLDER: Project Settings > Frame by Frame with the most important controls highlighted.]`

## Install the Package

Install the Unity package in your project. In this repository, it is embedded at:

```text
Packages/com.framebyframetool.integration
```

The package depends on `com.unity.nuget.newtonsoft-json` and several built-in Unity modules declared in `package.json`.

For Git URL installs, use:

```text
https://github.com/XDargu/FrameByFrame-UnityIntegration.git?path=/Packages/com.framebyframetool.integration#v1.0.0
```

## Start the Unity Server

1. Open **Window > Frame by Frame**.
2. Confirm the server status is **Running**.
3. If it is stopped, click **Start**.
4. Open Frame by Frame v1.1.0 and connect to the configured port.

Default connection settings:

```text
Port: 23001
Protocol: frameByframe
```

## Choose What to Record

Open **Edit > Project Settings > Frame by Frame**.

Enable only the recording options needed for the current debugging session. For example:

- enable `Navigation` to inspect `NavMeshAgent` paths
- enable `Colliders` to inspect collider shapes
- enable `Animations` to inspect Animator state
- enable `Log` to inspect Unity log messages

`[IMAGE PLACEHOLDER: Recording Options list with Navigation and Colliders enabled.]`

## Add Recorder Components

Add recorder components to the GameObjects you want to inspect.

Common components:

- `RecordPhysics`
- `RecordAI`
- `RecordNavMesh`
- `RecordAnimations`
- `RecordLog`
- `RecordPlane`
- `RecordScreenshot`

Then enter Play Mode. The selected data appears in Frame by Frame while the game runs.

## Keep the Connection Alive

Enable **Keep Alive Across Play Mode** in Project Settings to keep the websocket server running when entering and exiting Play Mode. This makes iteration smoother because the Frame by Frame viewer does not need to reconnect each time.
