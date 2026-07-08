# Publishing Frame by Frame for Unity

This checklist covers publishing `com.framebyframetool.integration` to GitHub and the Unity Asset Store.

## Package Identity

```text
Name: com.framebyframetool.integration
Display name: Frame by Frame for Unity
Version: 1.0.0
Supported viewer: Frame by Frame v1.1.0
Unity version: 2022.3+
License: MIT
```

## GitHub Release

1. Confirm the package compiles in a clean Unity 2022.3 project.
2. Confirm the package imports from:

   ```text
   https://github.com/XDargu/FrameByFrame-UnityIntegration.git?path=/Packages/com.framebyframetool.integration
   ```

3. Import the Demo sample from Package Manager.
4. Connect Frame by Frame v1.1.0.
5. Verify Play Mode recording options, editor window controls, and raw recording controls.
6. Verify a Development Build can start the server when build settings allow it.
7. Verify a non-development build does not start the server when **Development Builds Only** is enabled.
8. Commit the release changes.
9. Create and push tag:

   ```text
   v1.0.0
   ```

10. Create a GitHub release using the notes from `CHANGELOG.md`.

## Unity Asset Store Submission

1. Create a clean submission project.
2. Import or embed `Packages/com.framebyframetool.integration`.
3. Include sample content and documentation.
4. Use `Branding/Icon.png` as the store icon/source artwork.
5. Use `Branding/ExampleScreen.png` as a primary screenshot.
6. Capture the remaining screenshots listed as placeholders in the docs.
7. Confirm all included third-party notices are accurate.
8. Confirm package metadata:

   ```text
   Publisher: Frame by Frame / Daniel Armesto Gutierrez
   Version: 1.0.0
   Unity compatibility: 2022.3+
   License: MIT
   ```

9. Include a note that the runtime server is intended for editor and internal/development builds, not public release builds.
10. Submit through the Unity Asset Store Publisher portal.

## Screenshot Placeholders To Replace

- Editor window connected to Frame by Frame.
- Project Settings with connection and recording option controls.
- Recording Options list with selected options.
- Inspector showing recorder components.
- Frame by Frame timeline/event detail.
- Build Settings with Development Build enabled.
