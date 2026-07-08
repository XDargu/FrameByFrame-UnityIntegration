# API Reference

`[IMAGE PLACEHOLDER: Code editor showing a custom recorder next to the Frame by Frame viewer.]`

## Recording Option Attribute

```csharp
[FrameByFrameRecordingOption("Combat", "Records combat debug data.")]
public class RecordCombat : MonoBehaviour
{
}
```

The attribute declares a recording option before runtime component initialization.

Constructor:

```csharp
FrameByFrameRecordingOptionAttribute(string id, string description)
```

## FbFManager

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

### IsRecordingOptionEnabled

```csharp
bool enabled = FbFManager.IsRecordingOptionEnabled("Navigation");
```

Returns whether an option is currently enabled.

### SetRecordingOption

```csharp
FbFManager.SetRecordingOption("Navigation", true);
```

Changes a live recording option.

### RegisterRecordingOption

```csharp
FbFManager.RegisterRecordingOption("MyOption", "Records my custom data.");
```

Registers an option manually. Prefer `FrameByFrameRecordingOption` for options known at compile time.

### RecordEntity

```csharp
EntityData entity = FbFManager.RecordEntity(gameObject);
```

Gets or creates Frame by Frame entity data for a GameObject.

### RecordProperties

```csharp
PropertyGroup group = FbFManager.RecordProperties(gameObject, "Gameplay");
group.AddProperty("Health", 100);
```

Records grouped properties on an entity.

### RecordEvent

```csharp
PropertyGroup evt = FbFManager.RecordEvent(gameObject, "Damage Received", "Combat");
evt.AddProperty("Amount", 25);
```

Records an event associated with an entity.

### RecordResource

```csharp
FbFManager.RecordResource("debug.jpg", "image/jpg", jsonPayload);
```

Records a resource payload, such as an encoded image.

## FbFRecordingOptions

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
