# SECTOR X-13 — Delivery & Handover Notes

## 1. Canonical Entry Scene & Play Mode Walkthrough
1. Open `Assets/Scenes/X13_Modules/X13_01_Splash.unity` in Unity `6000.3.28f1`.
2. Press **Play** (Editor Simulation mode works without physical AR hardware):
   - `01_Splash` auto-advances (or tap **ENTER LABORATORY ->**) to `02_Home`.
   - `02_Home` displays **No completed experiment in this session** (never fabricates history).
   - Tap **OPEN CHEMICAL LIBRARY** (`03_Chemical_Library`) to inspect `CH-01`–`CH-10` across all 6 profile tabs.
   - Tap **START EXPERIMENT ->** (`04_Chemical_Selection`) to deliberately select Chemical A and Chemical B.
   - Tap **START SAFETY ASSESSMENT ->** (`05_Safety_Assessment`): unreviewed real scenarios (`EXP-01`, `EXP-02`, `EXP-03`) fail closed as `UNKNOWN · NOT VERIFIED` and block mixing.
   - Tap **OPEN SMART SAFETY ADVISOR ->** (`06_Safety_Advisor`): shows `No Reviewed Alternative Available` when no reviewed link exists.
   - When an authorized scenario is active (e.g., after scientific review or using `TEST ONLY` fixtures), `07_AR_Laboratory` enables **POUR**, **MIX**, **RESULTS ->**, and **RESET**.
   - `08_Experiment_Results` displays the completed scenario's equation, observations, safety notes, and educational outcome, with **RESET & RETURN HOME**.

## 2. Module Locations
- **Core & State**: `Assets/Scripts/SectorX13/Core/` (`SceneNavigationController.cs`, `ChemicalSelectionManager.cs`, `SafetyAssessment.cs`, `SmartSafetyAdvisor.cs`, `ExperimentManager.cs`, `X13SceneIds.cs`, `X13Theme.cs`)
- **Data Model**: `Assets/Scripts/SectorX13/Data/` (`ChemicalData.cs`, `ExperimentData.cs`, `AlternativeLink.cs`, `ChemicalDatabase.cs`)
- **UI Views**: `Assets/Scripts/SectorX13/UI/` (`X13ScreenController.cs`, `X13UIBuilderRuntime.cs`, `ChemicalLibraryView.cs`, `ChemicalSelectionView.cs`, `SafetyAssessmentView.cs`, `SafetyAdvisorView.cs`, `ARLabHUDView.cs`, `ExperimentResultsView.cs`)
- **AR & 3D Lab**: `Assets/Scripts/SectorX13/AR/ARPlacementManager.cs`, `Assets/Scripts/SectorX13/Lab/ReactionController.cs`
- **Editor Tools**: `Assets/Editor/` (`X13Phase2SceneBuilder.cs`, `X13DatasetImporter.cs`, `X13Phase6LabBuilder.cs`, `X13AndroidBuild.cs`)
- **Tests**: `Assets/Tests/EditMode/` (67 NUnit tests)

## 3. Adding Reviewed Scenarios
1. Edit `Assets/Data/x13_dataset.json` with scientifically approved `reviewStatus: "ReviewedApproved"`, `reviewedRiskCategory` (`Low`, `Medium`, or `High`), `reviewEvidence`, `dataVersion`, fixed quantities/conditions, and `effectId`.
2. In Unity Editor, run **SECTOR X-13 -> Phase 3 -> Import Dataset JSON** (or via CLI `-executeMethod SectorX13.Editor.X13DatasetImporter.ImportDataset`) to regenerate `Assets/Resources/X13Data/*.asset`.

## 4. Building Android APK
- **From Unity Editor**: Click **SECTOR X-13 -> Phase 8 -> Build Android APK** (outputs `Builds/Sector_X13.apk`).
- **From Unity CLI** (ensure Unity Editor is closed so `Temp/UnityLockfile` is free):
  ```powershell
  & "D:\unity project\6000.3.28f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "d:\unity project\Sector_X13" -executeMethod SectorX13.Editor.X13AndroidBuild.BuildAPK -logFile "d:\unity project\Sector_X13\Logs\phase8_android_build.log"
  ```
- Physical Android ARCore tracking and frame-rate/memory profiling require testing on a physical ARCore-supported Android device.
