# SECTOR X-13 — Implementation Plan

1. **Phase 1 — Project Audit & Environment Stabilization** (`feature/phase-01-audit-env`)
   - Git initialization, `.gitignore`, baseline tag `v0.0.0-baseline`, `develop` branch, Unity CLI compilation verification, Android `ProjectSettings` audit, documentation setup.
2. **Phase 2 — UI/UX and Navigation** (`feature/phase-02-ui-navigation`)
   - Automated uGUI generation across all 8 `X13_Modules` scenes matching the dark-navy/cyan Figma/PNG designs, responsive 9:16 portrait layout, navigation bar, and clean scene transitions.
3. **Phase 3 — Chemical Database & Information Module** (`feature/phase-03-chemical-database`)
   - `ChemicalData`, `ExperimentData`, `AlternativeLink` ScriptableObjects + dataset importer for `CH-01`–`CH-10` and `EXP-01`–`EXP-09`; 6-tab Chemical Profile and deliberate two-beaker selection.
4. **Phase 4 — Risk Assessment & Safety Engine** (`feature/phase-04-safety-engine`)
   - Fail-closed `SafetyAssessment` & `ExperimentManager` state machine (`Idle → Selected → Reviewing → Authorized/Prohibited → Simulating → Completed`) + isolated `TEST-01`–`TEST-09` unit tests.
5. **Phase 5 — Smart Safety Advisor** (`feature/phase-05-safety-advisor`)
   - Reviewed `AlternativeLink` lookup, clear "no alternative available" state, and reassessment from scratch on alternative selection.
6. **Phase 6 — AR Laboratory & 3D Simulation** (`feature/phase-06-ar-lab-3d`)
   - Fix URP speckle artifacts, 3-vessel lab (`Beaker A`, `Reaction Chamber`, `Beaker B`), `ReactionController` (`Pour`/`Mix`/`Reset` with safety-gate enforcement), and `ARPlacementManager` (`ARSession`/`XROrigin` + Editor simulation fallback).
7. **Phase 7 — Experiment Results & Full Integration** (`feature/phase-07-results-integration`)
   - Complete `08_Experiment_Results`, full state reset, and gate-bypass prevention across all 8 scenes.
8. **Phase 8 — Final Testing, Optimization & Deployment** (`feature/phase-08-testing-release`)
   - Execute 50-case functional test plan, regression tests, Android APK build via Unity CLI, and final release documentation.
