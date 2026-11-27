# SECTOR X-13 — Project Analysis

## 1. Environment & Packages
- **Unity Version**: `6000.3.28f1 (8f082d2fe476)`
- **Render Pipeline**: URP `17.3.0` (`Mobile_RPAsset.asset`, `PC_RPAsset.asset`)
- **XR / AR**: `com.unity.xr.arfoundation` `6.3.5`, `com.unity.xr.arcore` `6.3.5`
- **UI**: `com.unity.ugui` `2.0.0`, `com.unity.inputsystem` `1.20.1`
- **Testing**: `com.unity.test-framework` `1.6.0`
- **Android SDK**: `D:\Android_SDK` (`platforms/android-35`, `android-36`, `build-tools/35.0.0`–`36.1.0`, `ndk/27.1.12297006`, `28.2.13676358`)

## 2. Existing Scenes & Scripts
- **Canonical 8 Scenes**: `Assets/Scenes/X13_Modules/X13_01_Splash.unity` through `X13_08_Experiment_Results.unity`.
- **Legacy/Duplicate Scenes**: `Assets/AR_Laboratory.unity`, `Assets/Scene-1.unity`, `Assets/Scenes/SampleScene.unity`, `Assets/Scenes/X13_Laboratory_Simulation.unity`.
- **Runtime Scripts**:
  - `Assets/Scripts/X13MultiScene.cs` — `OnGUI()` prototype with static `X13State`.
  - `Assets/Scripts/X13HybridLab.cs` — Primitive 3D beaker animation layer in `X13_07_AR_Laboratory.unity`.
  - `Assets/Scripts/SectorX13Simulation.cs` — Single-scene IMGUI prototype.
- **Editor Scripts**: `SectorX13Builder.cs`, `X13HybridLabBuilder.cs`, `X13MultiSceneBuilder.cs`.

## 3. Critical Defects & Gaps
1. **Fail-Open Safety Bug**: `X13State.Assess()` hardcodes `HCl + NaOH` as `MEDIUM` and permits mixing despite `review_status = Pending`.
2. **Smart Safety Advisor Bug**: Hardcodes a pending preset instead of querying independently reviewed `AlternativeLink` records.
3. **Selection & State Bugs**: Defaults to index `0` for both reactants instead of requiring explicit selection; Home screen implies recent experiment data when none exists.
4. **UI/UX Mismatch**: Uses `OnGUI()` and static artwork overlays rather than responsive portrait uGUI matching the 8 Figma/PNG screens.
5. **3D Lab Visual Artifacts**: White/cyan speckle noise in the 3D viewport and floating stick pour stream; no `ARSession`/`XROrigin` placement manager.
6. **Missing Data Layer**: No `ChemicalData`/`ExperimentData`/`AlternativeLink` `ScriptableObject`s or dataset importer; missing `CH-10` and 6-tab Chemical Profile.
