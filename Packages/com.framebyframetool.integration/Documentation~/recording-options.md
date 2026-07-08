# Recording Options

Recording options control which channels of data may be recorded.

Options answer this question:

```text
What kind of data is allowed to be recorded right now?
```

They do not answer:

```text
Which objects are eligible?
```

Object selection should be handled by scene setup, recorder components, scopes, filters, layers, tags, or project-specific logic.

`[IMAGE PLACEHOLDER: Diagram showing Recording Options selecting data channels and scene filters selecting objects.]`

## Declaring an Option

Use `FrameByFrameRecordingOption`.

```csharp
using FbF;
using UnityEngine;

[FrameByFrameRecordingOption("Combat", "Records combat state, target links, and combat events.")]
public class RecordCombat : MonoBehaviour
{
	private void Update()
	{
		if (!FbFManager.IsRecordingOptionEnabled("Combat"))
		{
			return;
		}

		PropertyGroup group = FbFManager.RecordProperties(gameObject, "Combat");
		group.AddProperty("Is Attacking", true);
	}
}
```

The attribute has two values:

- `id`: stable option ID used by code and the Frame by Frame viewer
- `description`: human-readable explanation shown in Unity UI and sent with option metadata

There are no labels or categories. Keep IDs short, stable, and readable.

## Discovery

The plugin scans loaded assemblies for `FrameByFrameRecordingOption` attributes.

This happens:

- when the Unity editor loads
- before scene load in player builds
- when the editor window or Project Settings asks for option definitions

Because options are discovered from metadata, they can appear in Project Settings before Play Mode starts.

## Built-in Option IDs

Built-in option IDs are available as constants:

```csharp
FrameByFrameBuiltInRecordingOptions.Animations
FrameByFrameBuiltInRecordingOptions.Colliders
FrameByFrameBuiltInRecordingOptions.Collisions
FrameByFrameBuiltInRecordingOptions.Log
FrameByFrameBuiltInRecordingOptions.Navigation
FrameByFrameBuiltInRecordingOptions.NavMesh
FrameByFrameBuiltInRecordingOptions.Physics
FrameByFrameBuiltInRecordingOptions.ShapeHelpers
```

Use constants when referencing built-in options:

```csharp
if (FbFManager.IsRecordingOptionEnabled(FrameByFrameBuiltInRecordingOptions.Navigation))
{
	// Record navigation data.
}
```

## Defaults

Project Settings stores default enabled states for discovered options.

When settings are saved, Unity writes a runtime settings asset:

```text
Assets/FrameByFrame/Resources/FrameByFrameRuntimeSettings.asset
```

Development builds load this asset before the Frame by Frame server starts.

## Runtime Changes

Options can change at runtime through:

- the Unity editor window
- the Frame by Frame viewer
- game code calling `FbFManager.SetRecordingOption`

```csharp
FbFManager.SetRecordingOption("Combat", true);
```

Runtime changes affect live recording immediately.
