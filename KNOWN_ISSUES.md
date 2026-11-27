# Known Issues

1. **Unity Editor Lockfile**: Another Unity Editor instance currently holds `Temp/UnityLockfile`, blocking Unity CLI `-batchmode` execution until closed.
2. **Fail-Open Safety Logic (Existing Prototype)**: `X13MultiScene.cs` currently marks `HCl + NaOH` as `MEDIUM` despite pending scientific review. Scheduled for fix in Phase 4.
3. **3D Viewport Speckle Artifacts**: `X13_07_AR_Laboratory` exhibits white/cyan speckle noise in Game view. Scheduled for fix in Phase 6.
4. **Empty Android Application Identifier**: `applicationIdentifier` is unset in `ProjectSettings.asset`. Scheduled for fix in Phase 1/8.
5. **Scientific Review Pending**: All real chemical/experiment records (`CH-01`–`CH-10`, `EXP-01`–`EXP-09`) are pending scientific review; normal mode will fail closed (`UNKNOWN` / blocked) until reviewed records are supplied.
