# API Reference

`[IMAGE PLACEHOLDER: Code editor showing a custom recorder next to the Frame by Frame viewer with properties, an event, and rendered shapes selected.]`

The runtime API is in the `FbF` namespace. Most integrations follow the same pattern:

1. Declare or register a recording option.
2. Check whether that option is enabled.
3. Record properties for continuous state.
4. Record events for things that happened.
5. Add values, references, tables, comments, or shapes to the returned `PropertyGroup`.

## Recording Options

Recording options are named switches. They let users decide which categories of data are active before Play Mode and while the Frame by Frame viewer is connected.

### FrameByFrameRecordingOption

```csharp
[FrameByFrameRecordingOption("Combat", "Records combat debug data.")]
public sealed class RecordCombatDebug : MonoBehaviour
{
}
```

Use the attribute for options known at compile time. The editor discovers these attributes before Play Mode, so users can configure defaults in **Edit > Project Settings > Frame by Frame**.

The constructor is:

```csharp
FrameByFrameRecordingOptionAttribute(string id, string description)
```

The `id` is a stable key. Use the same value when checking `IsRecordingOptionEnabled`, setting option state, or tagging related data.

### RegisterRecordingOption

```csharp
FbFManager.RegisterRecordingOption("Streaming", "Records world streaming state.");
```

Registers an option manually. Use this for dynamic systems where an attribute is not practical. Prefer the attribute for normal `MonoBehaviour` recorders.

### IsRecordingOptionEnabled

```csharp
if (!FbFManager.IsRecordingOptionEnabled("Combat"))
{
	return;
}
```

Returns whether an option is currently enabled. Custom recorders should usually check this before doing expensive work or writing data to the current frame.

### SetRecordingOption

```csharp
FbFManager.SetRecordingOption("Combat", true);
```

Changes a live option state. This is useful for internal debug UI, test harnesses, or automated capture scenarios. The Frame by Frame viewer can also change option state while connected.

## Entities

### RecordEntity

```csharp
EntityData entity = FbFManager.RecordEntity(gameObject);
```

Records a Unity `GameObject` into the current frame and returns its entity data. The integration records the object's name, transform position, up vector, forward vector, and parent relationship.

Most recorders do not need to call this directly. `RecordProperties` and `RecordEvent` call it automatically.

## Properties

### RecordProperties

```csharp
PropertyGroup group = FbFManager.RecordProperties(gameObject, "Combat");
group.AddProperty("Health", health);
group.AddProperty("Is Reloading", isReloading);
group.AddProperty("Aim Direction", transform.forward);
```

Properties describe the state of an entity for the current frame. Use them for values that are meaningful while scrubbing a recording: AI state, animation state, physics state, gameplay stats, target references, sensor ranges, or rendered debug shapes.

Supported simple values:

- `bool`
- `string`
- `int`
- `float`
- `Vector2`
- `Vector3`

### AddProperty

```csharp
group.AddProperty("Health", health, new Icon("heart", "#ff4d4d"));
group.AddProperty("State", currentStateName);
group.AddProperty("Speed", speed, null, PropertyFlags.Collapsed);
```

Adds a simple scalar or vector value to a group. Icons and flags are optional. `PropertyFlags.Collapsed` is useful for verbose values that should be available without dominating the viewer.

### AddGroup

```csharp
PropertyGroup steering = group.AddGroup("Steering");
steering.AddProperty("Desired Velocity", desiredVelocity);
steering.AddProperty("Avoidance Weight", avoidanceWeight);
```

Adds a nested section. Use groups to keep data readable when a recorder emits several related values.

### AddComment

```csharp
group.AddComment("Waiting for path result.");
```

Adds a text note. Comments are useful for human-readable explanations, warnings, or short summaries.

### AddEntityRef

```csharp
group.AddEntityRef("Target", targetGameObject);
```

Adds a reference to another recorded entity. Use this for targets, owners, instigators, selected objects, parent systems, or any relationship that is easier to inspect as a link than as plain text.

### AddTable

```csharp
PropertyTable scores = group.AddTable("Utility Scores", "Consideration", "Score");
scores.AddRow("Cover", coverScore.ToString("0.00"));
scores.AddRow("Reload", reloadScore.ToString("0.00"));
```

Adds tabular data. Tables are a good fit for utility AI scores, candidate lists, query results, or ranked decisions.

## Events

### RecordEvent

```csharp
PropertyGroup evt = FbFManager.RecordEvent(gameObject, "Damage Received", "Combat");
evt.AddProperty("Amount", amount);
evt.AddProperty("Damage Type", damageType);
evt.AddEntityRef("Instigator", instigator);
```

Records a timeline event attached to an entity. The event name should describe what happened. The tag groups related events, such as `Combat`, `Collision`, `Navigation`, or `Log`.

The returned `PropertyGroup` works like a normal property group. Add enough context for another developer to understand the event without reproducing the bug.

Common event examples:

- `Damage Received`
- `Ability Started`
- `Goal Changed`
- `Path Failed`
- `Door Opened`
- `OnCollisionEnter`
- `Log Error`

## Shape Properties

Shape helpers add renderable 3D debug geometry to a property group. They are useful for inspecting physics, AI, navigation, perception, targeting, and spatial queries.

```csharp
PropertyGroup debug = FbFManager.RecordProperties(gameObject, "Navigation");

debug.AddPath("Current Path", pathCorners, Color.cyan, "Navigation");
debug.AddLine("Velocity", transform.position, transform.position + velocity, Color.blue, "Physics");
debug.AddSphere("Detection Radius", transform.position, detectionRadius, Color.yellow, "AI");
```

Available helpers:

- `AddSphere`
- `AddAABB`
- `AddOOBB`
- `AddCapsule`
- `AddPlane`
- `AddLine`
- `AddMesh`
- `AddPath`
- `AddTriangle`

Use the `layer` argument to group related shapes in the viewer. Recommended layer names are stable system names such as `Physics`, `Navigation`, `Combat`, `Cover`, `Perception`, or `ShapeHelpers`.

## Resources

### RecordResource

```csharp
FbFManager.RecordResource("screenshots/game-view.jpg", "image/jpg", encodedImageJson);
```

Records a named payload that can be referenced by properties. The built-in screenshot recorder uses this to send image data, then renders that image as a textured plane with `AddPlane`.

Example:

```csharp
FbFManager.RecordResource("debug/camera.jpg", "image/jpg", encodedImageJson);

PropertyGroup group = FbFManager.RecordProperties(gameObject, "Screenshot");
group.AddPlane(
	"Camera View",
	transform.position,
	transform.forward,
	transform.up,
	16.0f,
	9.0f,
	Color.white,
	"debug/camera.jpg",
	"ShapeHelpers");
```

## FbFRecordingOptions

`FbFRecordingOptions` is the lower-level registry behind the manager convenience methods.

### DiscoverOptions

```csharp
FbFRecordingOptions.DiscoverOptions();
```

Scans loaded assemblies for `FrameByFrameRecordingOption` attributes.

### Register

```csharp
FbFRecordingOptions.Register("Combat", "Records combat debug data.");
```

Registers an option definition.

### GetDefinitions

```csharp
List<FrameByFrameRecordingOptionDefinition> definitions = FbFRecordingOptions.GetDefinitions();
```

Returns discovered option definitions.

### SetEnabled

```csharp
FbFRecordingOptions.SetEnabled("Combat", true);
```

Changes an option state.

## Server Lifecycle

### InitializeForEditor

```csharp
FbFManager.InitializeForEditor();
```

Initializes the manager and starts the websocket server in the editor.

### StartServer

```csharp
FbFManager.StartServer();
```

Starts the websocket server. If the manager has not been initialized yet, this performs full initialization first.

### StopServer

```csharp
FbFManager.StopServer();
```

Stops the websocket server and shuts down the recorder.

## Runtime Settings

Development builds load:

```text
Resources/FrameByFrameRuntimeSettings
```

The asset stores:

- build enable flags
- port and protocol
- timing settings
- raw recording path
- default recording option states
