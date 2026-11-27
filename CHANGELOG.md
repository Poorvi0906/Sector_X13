# Changelog

## [v1.0.0-rc1] - 2026-11-27
- **Phase 2**: Replaced legacy `OnGUI()` prototypes with responsive 9:16 portrait uGUI across all 8 canonical `X13_Modules` scenes (`SceneNavigationController`, `X13Theme`, `X13UIBuilderRuntime`, `X13ScreenController`).
- **Phase 3**: Implemented `ChemicalData`, `ExperimentData`, `AlternativeLink`, `ChemicalDatabase`, `X13DatasetImporter`, 10-chemical catalogue (`CH-01`–`CH-10`), 6-section Chemical Profile, and deliberate two-beaker selection (`ChemicalSelectionManager`).
- **Phase 4**: Implemented fail-closed `SafetyAssessment`, `ExperimentManager` lifecycle state machine, scenario-bound `MEDIUM` acknowledgement gate, and isolated `TEST ONLY` fixtures.
- **Phase 5**: Implemented `SmartSafetyAdvisor` (independently reviewed, explicitly linked alternatives only; clears authorization and reassesses from scratch).
- **Phase 6**: Rebuilt `X13_07_AR_Laboratory.unity` with clean URP 3D vessels, `ARPlacementManager` (`ARSession`/`XROrigin` + Editor fallback), and `ReactionController` (safety-gated Pour/Mix/Reset + duplicate-start guard).
- **Phase 7**: Implemented `ExperimentResultsView` (no fabricated results), full `ResetSession()`, and direct-navigation gate-bypass prevention.
- **Phase 8**: Executed all 67 EditMode tests (including 50-case functional suite) with 67/67 passing; added `X13AndroidBuild.cs` and `HANDOVER_NOTES.md`.

## [v0.0.0-baseline] - 2026-11-27
- Initialized Git repository on `main` aligned with `origin/main`.
- Added Unity `.gitignore` preserving `.meta` files, `Assets/`, `Packages/`, and `ProjectSettings/`.
- Added initial project tracking documentation and verified Unity 6000.3.28f1 CLI compilation.
