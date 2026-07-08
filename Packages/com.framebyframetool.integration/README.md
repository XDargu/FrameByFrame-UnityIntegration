# Frame by Frame for Unity

Unity integration for Frame by Frame v1.1.0.

![Frame by Frame for Unity example](Documentation~/images/ExampleScreen.png)

`[IMAGE PLACEHOLDER: Frame by Frame viewer connected to a Unity scene, with recorded entities, properties, events, and debug shapes visible.]`

## Install

Install from Git URL:

```text
https://github.com/XDargu/FrameByFrame-UnityIntegration.git?path=/Packages/com.framebyframetool.integration#v1.0.0
```

Or install from the Unity Asset Store after approval.

## Start

1. Open **Window > Frame by Frame**.
2. Open **Edit > Project Settings > Frame by Frame**.
3. Enable the recording options you want.
4. Start Frame by Frame v1.1.0.
5. Enter Play Mode.
6. Add recorder components such as `RecordPhysics`, `RecordAI`, `RecordAnimations`, or `RecordLog` to the GameObjects you want to inspect.

## Core Concepts

- **Entities** are Unity `GameObject` instances recorded into the current frame.
- **Properties** are state values for the current frame, such as health, path status, velocity, target, or debug shapes.
- **Events** are timeline markers attached to an entity, such as damage received, collision entered, path failed, or log error.
- **Recording options** are switches that let teams decide which categories of data are active before Play Mode and while connected.
- **Resources** are named payloads, commonly used for image data referenced by textured planes.

Frame by Frame does not need to record the whole scene. A production recorder should combine recording options with local filters, such as distance to player, selected debug target, team, scenario, or gameplay relevance.

## Runtime API Overview

Most custom recorders use this flow:

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("Combat", "Records combat state and events.")]
public sealed class RecordCombatDebug : MonoBehaviour
{
	public int health;
	public GameObject target;

	private void Update()
	{
		if (!FbFManager.IsRecordingOptionEnabled("Combat"))
		{
			return;
		}

		PropertyGroup group = FbFManager.RecordProperties(gameObject, "Combat");
		group.AddProperty("Health", health, new Icon("heart"));

		if (target != null)
		{
			group.AddEntityRef("Target", target);
			group.AddLine("Target Direction", transform.position, target.transform.position, Color.red, "Combat");
		}
	}

	public void RecordDamage(int amount, GameObject instigator)
	{
		if (!FbFManager.IsRecordingOptionEnabled("Combat"))
		{
			return;
		}

		PropertyGroup evt = FbFManager.RecordEvent(gameObject, "Damage Received", "Combat");
		evt.AddProperty("Amount", amount);
		evt.AddEntityRef("Instigator", instigator);
	}
}
```

`RecordProperties` records state for the current frame. Add simple values with `AddProperty`, nested sections with `AddGroup`, references with `AddEntityRef`, notes with `AddComment`, and tables with `AddTable`.

`RecordEvent` creates a timeline entry for something that happened. The returned `PropertyGroup` lets you attach context to the event, such as damage amount, target, previous state, new state, or failure reason.

Shape helpers can render visual debug data in the Frame by Frame viewer:

```csharp
PropertyGroup debug = FbFManager.RecordProperties(gameObject, "Navigation");
debug.AddPath("Current Path", pathCorners, Color.cyan, "Navigation");
debug.AddSphere("Detection Radius", transform.position, radius, Color.yellow, "AI");
debug.AddLine("Velocity", transform.position, transform.position + velocity, Color.blue, "Physics");
```

Available helpers include `AddSphere`, `AddAABB`, `AddOOBB`, `AddCapsule`, `AddPlane`, `AddLine`, `AddMesh`, `AddPath`, and `AddTriangle`.

## More Documentation

See [Documentation~/README.md](Documentation~/README.md) for the full documentation:

- quick start
- recording options
- built-in recorders
- API reference
- examples
- internal builds
