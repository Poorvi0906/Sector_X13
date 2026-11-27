# SECTOR X-13 — Test Report (50-Case Functional + 17 Unit/Integration Tests)

**Test Execution Date**: 2026-11-27  
**Unity Version**: `6000.3.28f1`  
**Test Runner**: Unity Test Framework (`1.6.0`) — EditMode CLI (`Logs/phase8_test_results.xml`)  
**Result**: **67 / 67 Passed** (`0` Failed, `0` Skipped, Duration `4.39s`)

---

## 1. Automated Test Suite Summary

| Suite | Tests | Passed | Failed | Scope |
| :--- | :---: | :---: | :---: | :--- |
| `Phase2NavigationTests` | 3 | 3 | 0 | 8 canonical scenes in Build Settings, `X13ScreenController`, zero legacy `X13MultiScene` |
| `Phase3DatabaseTests` | 3 | 3 | 0 | 10-chemical catalogue (`CH-01`–`CH-10`), 6 profile tabs, `PendingReview` candidates, unselected A/B default |
| `Phase4SafetyTests` | 5 | 5 | 0 | Fail-closed `HCl+NaOH`, missing/unreviewed block, synthetic `LOW`/`MEDIUM`/`HIGH` gates |
| `Phase5AdvisorTests` | 2 | 2 | 0 | Normal mode empty advisor, synthetic `TEST-03 -> ALT-TEST-01 -> TEST-01` reassessment |
| `Phase6LabTests` | 2 | 2 | 0 | Unauthorized pour/mix block, authorized simulation + completion gate |
| `Phase7IntegrationTests` | 2 | 2 | 0 | Gate-bypass prevention, full lifecycle + `ResetSession()` |
| `Phase8FiftyFunctionalTests` | 50 | 50 | 0 | All 50 functional cases (`TC_GEN_01`–`10`, `TC_INF_01`–`06`, `TC_RSK_01`–`10`, `TC_ADV_01`–`08`, `TC_RXN_01`–`08`, `TC_ERR_01`–`08`) |
| **Total** | **67** | **67** | **0** | — |

---

## 2. Scientific Review & Limitations
- Real experiment candidates (`EXP-01`, `EXP-02`, `EXP-03`) remain `PendingReview` and `RiskCategory.Unknown` in `Assets/Data/x13_dataset.json`, so normal mode **fails closed** (`UNKNOWN` / blocked) as required.
- Software logic for `LOW`, `MEDIUM`, `HIGH`, and `SmartSafetyAdvisor` is verified using isolated synthetic `TEST ONLY` fixtures (`TEST-01`–`TEST-03`, `ALT-TEST-01`), which are excluded from the normal catalogue.
- Android APK CLI build (`SectorX13.Editor.X13AndroidBuild.BuildAPK`) was blocked at the final step because `Temp/UnityLockfile` is currently held by an open Unity Editor instance on the machine.
