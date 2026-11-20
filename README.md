# Unity AR Prototype

An individual university AR learning project by **Munise Aydogdu**, developed in the context of the FH Aachen VR/AR course.

This repository presents the application scripts for placing virtual objects on detected surfaces and experimenting with QR-triggered video playback in augmented reality.

## Status

**Source-code showcase / work in progress.** This is not a complete, ready-to-open Unity project. The original course scene, sample assets, plug-ins and test video are not redistributed here. No new device test or Unity build was performed when preparing this repository.

The original scene contains the surface-placement component. QR scanning and video playback are implemented in scripts, but their complete scene integration is unfinished. In particular, the video-surface registration still needs to be wired up and tested on an AR-capable device.

## Components

| Script | Responsibility |
| --- | --- |
| `ARObjectSpawner.cs` | Responds to a screen tap, raycasts against detected AR planes and instantiates a prefab at the hit pose. |
| `QRScanner.cs` | Reads camera frames through AR Foundation and decodes QR codes with ZXing; exposes an event when a code is found. |
| `QRController.cs` | Handles `sa:` video references, prepares a Unity VideoPlayer and sends video output to a registered surface. |
| `WallVideoSurface.cs` | Registers a renderer as the target for video playback. |

## Technologies

C#, Unity, AR Foundation, ARCore / ARKit, ZXing, Unity VideoPlayer, Universal Render Pipeline.

The local source project records Unity **6000.6.2f1** and AR Foundation / ARCore / ARKit **6.6.2**. These are source-project versions, not a claim that this standalone script collection has been built or verified with them.

## Integration outline

1. Create an AR-enabled Unity project and install the appropriate AR Foundation and platform-provider packages. Use a compatible AR-capable device.
2. Add the scripts from `Assets/Scripts` and supply a compatible ZXing dependency. Package and third-party dependencies must be obtained from their respective distributors.
3. Configure an AR Session, camera, plane detection and ARRaycastManager. Attach `ARObjectSpawner` to the object holding ARRaycastManager and assign `objectPrefab`.
4. For QR experiments, attach `QRScanner` to the AR camera with ARCameraManager, and add `QRController`. Grant camera access on the target platform.
5. Add `WallVideoSurface` to the intended video renderer. The current script looks for an object named `Main Camera` containing QRController; adjust the setup accordingly.
6. Supply your own video in `StreamingAssets` and a QR code containing a reference such as `sa:example.mp4`.

These steps describe the intended setup; they are not a verified end-to-end installation recipe.

## Known limitations / next steps

- Complete and verify QR-to-surface integration on a device.
- A QR code read before a surface is registered is currently not queued; handle this ordering explicitly.
- Resume scanning after a failed Android StreamingAssets copy.
- Clean up VideoPlayer event subscriptions before repeated playback.
- Add defensive checks for missing camera, prefab and AR components.
- Record tested device, platform and dependency versions after the next hardware session.

## Project context and attribution

The local working project originated from FH Aachen VR/AR course templates (`maskor/course_material/vrar_praktikum_aufgaben/vrar-aufgabenstellung-templates`). This repository separates the application-script portion from the Unity/XR sample environment. It does not claim authorship of Unity packages, XR starter assets or the ZXing library. This is a university learning prototype, not a production application.
