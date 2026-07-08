# Frame by Frame for Unity

This documentation covers the official Unity plugin for Frame by Frame v1.1.0.

![Frame by Frame for Unity example](images/ExampleScreen.png)

## Pages

- [Quick Start](quick-start.md)
- [Recording Options](recording-options.md)
- [Built-in Recorders](built-in-recorders.md)
- [API Reference](api-reference.md)
- [Examples](examples.md)
- [Internal Builds](internal-builds.md)

## What the Plugin Does

The Unity plugin opens a websocket server inside Unity, records selected runtime data, and streams frame data to the Frame by Frame viewer. It is designed for editor use and internal development builds.

The plugin does not automatically record every object. Developers choose what to record through recording options, recorder components, and project-specific filtering.
