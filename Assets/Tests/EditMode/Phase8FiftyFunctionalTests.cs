using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using SectorX13.Lab;
using SectorX13.UI;
using UnityEditor;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    /// <summary>
    /// 50-case functional verification suite mapped 1:1 to PDF Page 7 test groups:
    /// General (10), Chemical Info (6), Risk (10), Advisor (8), Reaction (8), Error Handling (8).
    /// </summary>
    public class Phase8FiftyFunctionalTests
    {
        private GameObject _host;
        private SceneNavigationController _nav;
        private ChemicalSelectionManager _sel;
        private ExperimentManager _exp;
        private ReactionController _lab;
        private ChemicalDatabase _db;

        [SetUp]
        public void SetUp()
        {
            ChemicalDatabase.ClearCache();
            _db = ChemicalDatabase.Load();
            _host = new GameObject("Phase8TestHost");
            _nav = _host.AddComponent<SceneNavigationController>();
            _sel = _host.AddComponent<ChemicalSelectionManager>();
            _exp = _host.AddComponent<ExperimentManager>();
            _exp.EnsureInitialized();
            _lab = _host.AddComponent<ReactionController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        // --- GENERAL (10) ---
        [Test] public void TC_GEN_01_LaunchCanonicalEntryScene() => Assert.AreEqual(X13SceneIds.GetScenePath(X13SceneIds.Splash), EditorBuildSettings.scenes[0].path);
        [Test] public void TC_GEN_02_HomeNavigation() => Assert.IsTrue(_nav.NavigateTo(X13SceneIds.Home));
        [Test] public void TC_GEN_03_CatalogueNavigation() => Assert.IsTrue(_nav.NavigateTo(X13SceneIds.ChemicalLibrary));
        [Test] public void TC_GEN_04_SelectionNavigation() => Assert.IsTrue(_nav.NavigateTo(X13SceneIds.ChemicalSelection));
        [Test] public void TC_GEN_05_ABSelectionStartsEmpty() { Assert.IsNull(_sel.ChemicalAId); Assert.IsNull(_sel.ChemicalBId); }
        [Test] public void TC_GEN_06_StartRequiresBothSelections() { _sel.SelectChemicalA("CH-01"); Assert.IsFalse(_sel.HasBothSelections); }
        [Test] public void TC_GEN_07_SafetyDecisionExecutes() { _sel.SelectPair("CH-02", "CH-05"); Assert.IsNotNull(_exp.AssessCurrentSelection()); }
        [Test] public void TC_GEN_08_ResetClearsState() { _sel.SelectPair("CH-01", "CH-09"); _exp.ResetSession(); Assert.AreEqual(ExperimentLifecycleState.Idle, _exp.State); }
        [Test] public void TC_GEN_09_ReturnHomeClearsHistory() { _nav.NavigateTo(X13SceneIds.ChemicalLibrary); _nav.GoHome(true); Assert.AreEqual(0, _nav.HistoryCount); }
        [Test] public void TC_GEN_10_BackHistoryPopsPrevious() { _nav.NavigateTo(X13SceneIds.Home); _nav.NavigateTo(X13SceneIds.ChemicalLibrary); _nav.GoBack(); Assert.AreEqual(X13SceneIds.Home, _nav.CurrentScene); }

        // --- CHEMICAL INFORMATION (6) ---
        [Test] public void TC_INF_01_SectionBasicInfo() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-01"), 0).Contains("Acetic Acid"));
        [Test] public void TC_INF_02_SectionProperties() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-02"), 1).Contains("Form:"));
        [Test] public void TC_INF_03_SectionSafetyInfo() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-04"), 2).Contains("Hazards:"));
        [Test] public void TC_INF_04_SectionExperimentInfo() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-05"), 3).Contains("EXP-02"));
        [Test] public void TC_INF_05_SectionARInfo() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-09"), 4).Contains("Visual Asset ID:"));
        [Test] public void TC_INF_06_SectionRiskAssessment() => Assert.IsTrue(ChemicalLibraryView.GetSectionContent(_db.GetChemicalById("CH-10"), 5).Contains("Assessment:"));

        // --- RISK (10) ---
        [Test] public void TC_RSK_01_LowPermits() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); Assert.AreEqual(RiskCategory.Low, _exp.AssessCurrentSelection().Risk); Assert.IsTrue(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_02_MediumBlocksBeforeAck() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-C"); _exp.AssessCurrentSelection(); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_03_MediumPermitsAfterAck() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-C"); var r = _exp.AssessCurrentSelection(); Assert.IsTrue(_exp.AcknowledgeMediumRisk(r.ScenarioId, r.DataVersion)); Assert.IsTrue(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_04_HighBlocks() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-D"); Assert.AreEqual(RiskCategory.High, _exp.AssessCurrentSelection().Risk); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_05_UnknownBlocks() { _sel.SelectPair("CH-04", "CH-06"); Assert.AreEqual(RiskCategory.Unknown, _exp.AssessCurrentSelection().Risk); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_06_MissingRecordBlocks() { _sel.SelectPair("CH-07", "CH-08"); Assert.IsFalse(_exp.AssessCurrentSelection().IsAuthorizedByRule); }
        [Test] public void TC_RSK_07_UnreviewedPendingBlocks() { _sel.SelectPair("CH-02", "CH-05"); Assert.AreEqual(RiskCategory.Unknown, _exp.AssessCurrentSelection().Risk); }
        [Test] public void TC_RSK_08_IncompleteRecordBlocks() { var exp = ScriptableObject.CreateInstance<ExperimentData>(); Assert.IsFalse(exp.IsCompleteForExecution()); Object.DestroyImmediate(exp); }
        [Test] public void TC_RSK_09_ChangedSelectionInvalidatesAuth() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); _sel.SelectChemicalB("TEST-C"); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }
        [Test] public void TC_RSK_10_ResetInvalidatesAuth() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); _exp.ResetSession(); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }

        // --- ADVISOR (8) ---
        [Test] public void TC_ADV_01_EligibleAlternativeFound() { _sel.IncludeSyntheticTestRecords = true; var list = SmartSafetyAdvisor.GetEligibleAlternatives(_db, "TEST-03", "TEST-A", "TEST-D", true); Assert.AreEqual(1, list.Count); }
        [Test] public void TC_ADV_02_NoAlternativeInNormalMode() => Assert.AreEqual(0, SmartSafetyAdvisor.GetEligibleAlternatives(_db, "EXP-02", "CH-02", "CH-05", false).Count);
        [Test] public void TC_ADV_03_UnreviewedLinkRejected() => Assert.AreEqual(0, SmartSafetyAdvisor.GetEligibleAlternatives(_db, "TEST-03", "TEST-A", "TEST-D", false).Count);
        [Test] public void TC_ADV_04_IncompleteLinkRejected() { var link = ScriptableObject.CreateInstance<AlternativeLink>(); Assert.IsFalse(link.IsComplete()); Object.DestroyImmediate(link); }
        [Test] public void TC_ADV_05_InvalidTargetRejected() { var list = SmartSafetyAdvisor.GetEligibleAlternatives(_db, "NONEXISTENT", "X", "Y", true); Assert.AreEqual(0, list.Count); }
        [Test] public void TC_ADV_06_SelectionClearsAuth() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); var list = SmartSafetyAdvisor.GetEligibleAlternatives(_db, "TEST-03", "TEST-A", "TEST-D", true); SmartSafetyAdvisor.ApplyAlternative(list[0]); Assert.IsNull(_exp.CurrentAssessment); }
        [Test] public void TC_ADV_07_ReassessmentFromScratch() { _sel.IncludeSyntheticTestRecords = true; var list = SmartSafetyAdvisor.GetEligibleAlternatives(_db, "TEST-03", "TEST-A", "TEST-D", true); SmartSafetyAdvisor.ApplyAlternative(list[0]); Assert.AreEqual(RiskCategory.Low, _exp.AssessCurrentSelection().Risk); }
        [Test] public void TC_ADV_08_OriginalRestrictionPreservedIfNoAlt() { _sel.SelectPair("CH-02", "CH-05"); _exp.AssessCurrentSelection(); Assert.IsFalse(_exp.VerifyAuthorizationForExecution()); }

        // --- REACTION (8) ---
        [Test] public void TC_RXN_01_AuthorizedEffectStarts() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); Assert.IsTrue(_exp.BeginSimulation()); }
        [Test] public void TC_RXN_02_UnauthorizedBlock() { Assert.IsFalse(_lab.TriggerPour()); Assert.IsFalse(_lab.TriggerMix()); }
        [Test] public void TC_RXN_03_MediumGateEnforced() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-C"); _exp.AssessCurrentSelection(); Assert.IsFalse(_lab.TriggerMix()); }
        [Test] public void TC_RXN_04_HighGateBlocked() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-D"); _exp.AssessCurrentSelection(); Assert.IsFalse(_lab.TriggerMix()); }
        [Test] public void TC_RXN_05_UnknownGateBlocked() { _sel.SelectPair("CH-02", "CH-05"); _exp.AssessCurrentSelection(); Assert.IsFalse(_lab.TriggerMix()); }
        [Test] public void TC_RXN_06_ResultMatchesCompletedScenario() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); _exp.BeginSimulation(); _exp.CompleteSimulation(); Assert.AreEqual("TEST-01", _exp.CurrentAssessment.BoundExperiment.scenarioId); }
        [Test] public void TC_RXN_07_ResetClearsLabAndSession() { _lab.ResetVisuals(); _exp.ResetSession(); Assert.IsFalse(_lab.HasPoured); Assert.IsFalse(_exp.HasCompletedScenario); }
        [Test] public void TC_RXN_08_RepeatStartGuard() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); _lab.TriggerMix(); Assert.IsFalse(_lab.TriggerMix()); }

        // --- ERROR HANDLING (8) ---
        [Test] public void TC_ERR_01_MissingA() { _sel.SelectChemicalB("CH-05"); Assert.IsFalse(SafetyAssessment.Evaluate(_db, _sel.ChemicalAId, _sel.ChemicalBId).IsAuthorizedByRule); }
        [Test] public void TC_ERR_02_MissingB() { _sel.SelectChemicalA("CH-02"); Assert.IsFalse(SafetyAssessment.Evaluate(_db, _sel.ChemicalAId, _sel.ChemicalBId).IsAuthorizedByRule); }
        [Test] public void TC_ERR_03_MissingChemRecord() => Assert.IsNull(_db.GetChemicalById("CH-999"));
        [Test] public void TC_ERR_04_MissingExpRecord() => Assert.IsNull(_db.GetExperimentById("EXP-999"));
        [Test] public void TC_ERR_05_InvalidIdenticalPairBlocked() => Assert.AreEqual(RiskCategory.Unknown, SafetyAssessment.Evaluate(_db, "CH-01", "CH-01").Risk);
        [Test] public void TC_ERR_06_NullDatabaseFailsClosed() => Assert.IsFalse(SafetyAssessment.Evaluate(null, "CH-01", "CH-09").IsAuthorizedByRule);
        [Test] public void TC_ERR_07_InvalidAlternativeNullSafe() => Assert.IsFalse(SmartSafetyAdvisor.ApplyAlternative(null));
        [Test] public void TC_ERR_08_RepeatedPourGuard() { _sel.IncludeSyntheticTestRecords = true; _sel.SelectPair("TEST-A", "TEST-B"); _exp.AssessCurrentSelection(); _lab.TriggerPour(); Assert.IsFalse(_lab.TriggerPour()); }
    }
}
