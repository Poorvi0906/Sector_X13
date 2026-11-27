using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase5AdvisorTests
    {
        private GameObject _host;
        private ChemicalSelectionManager _sel;
        private ExperimentManager _exp;

        [SetUp]
        public void SetUp()
        {
            ChemicalDatabase.ClearCache();
            _host = new GameObject("Phase5TestHost");
            _host.AddComponent<SceneNavigationController>();
            _sel = _host.AddComponent<ChemicalSelectionManager>();
            _exp = _host.AddComponent<ExperimentManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void NormalMode_ReturnsNoAlternatives_ForUnreviewedPairs()
        {
            var db = ChemicalDatabase.Load();
            var list = SmartSafetyAdvisor.GetEligibleAlternatives(db, "EXP-02", "CH-02", "CH-05", allowSyntheticTestMode: false);
            Assert.AreEqual(0, list.Count, "Pending/unreviewed pairs must not expose unreviewed alternatives in normal mode.");
        }

        [Test]
        public void SyntheticHighRisk_FindsReviewedAlternative_AndReassessesFromScratch()
        {
            _sel.IncludeSyntheticTestRecords = true;
            _sel.SelectPair("TEST-A", "TEST-D"); // TEST-03 (High)
            var assessment = _exp.AssessCurrentSelection();
            Assert.AreEqual(RiskCategory.High, assessment.Risk);

            var db = ChemicalDatabase.Load();
            var list = SmartSafetyAdvisor.GetEligibleAlternatives(db, assessment.ScenarioId, "TEST-A", "TEST-D", allowSyntheticTestMode: true);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("TEST-01", list[0].TargetExperiment.scenarioId);

            Assert.IsTrue(SmartSafetyAdvisor.ApplyAlternative(list[0]));
            Assert.AreEqual("TEST-A", _sel.ChemicalAId);
            Assert.AreEqual("TEST-B", _sel.ChemicalBId);
            Assert.IsNull(_exp.CurrentAssessment, "Applying alternative must clear previous assessment for fresh reassessment.");

            var reassessed = _exp.AssessCurrentSelection();
            Assert.AreEqual(RiskCategory.Low, reassessed.Risk);
            Assert.IsTrue(_exp.VerifyAuthorizationForExecution());
        }
    }
}
