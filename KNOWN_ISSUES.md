# Known Issues

1. **Fail-Open Safety Logic (Existing Prototype)**: `X13MultiScene.cs` currently marks `HCl + NaOH` as `MEDIUM` despite pending scientific review. Scheduled for fix in Phase 4.
2. **3D Viewport Speckle Artifacts**: `X13_07_AR_Laboratory` exhibits white/cyan speckle noise in Game view. Scheduled for fix in Phase 6.
3. **Empty Android Application Identifier**: `applicationIdentifier` is unset in `ProjectSettings.asset`. Scheduled for fix in Phase 8 (or earlier during Android config).
4. **Scientific Review Pending**: All real chemical/experiment records (`CH-01`–`CH-10`, `EXP-01`–`EXP-09`) are pending scientific review; normal mode will fail closed (`UNKNOWN` / blocked) until reviewed records are supplied.
