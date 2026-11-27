# SECTOR X-13: An AR-Based Safe Chemistry Experimentation System

Educational virtual chemistry laboratory built with **Unity 6000.3.28f1**, **C#**, **URP 17.3.0**, **AR Foundation 6.3.5**, and **ARCore 6.3.5** (Android target + Unity Editor Play Mode simulation).

## Safety & Scientific Scope
- **Virtual Educational Simulation Only**: This application is not a chemical prediction engine and does not authorize physical laboratory experiments.
- **Fail-Closed Safety Gate**: Only independently reviewed, complete, versioned scenario records may permit virtual mixing. Any pending, missing, incomplete, or unreviewed record defaults to `UNKNOWN` and blocks mixing.
- **Synthetic Test Fixtures**: Isolated `TEST-01` through `TEST-09` fixtures use fictional identifiers (`TEST-A`..`TEST-J`) for software-logic verification only.

## Project Environment
- **Unity Editor**: `6000.3.28f1`
- **Target Platform**: Android (ARCore) + Unity Editor Play Mode
- **Canonical Entry Scene**: `Assets/Scenes/X13_Modules/X13_01_Splash.unity`
- **Android SDK**: `D:\Android_SDK`

## Repository Branches
- `main` — Stable, tested milestone releases only
- `develop` — Integration branch for verified phases
- `feature/phase-XX-*` — Individual phase development branches
