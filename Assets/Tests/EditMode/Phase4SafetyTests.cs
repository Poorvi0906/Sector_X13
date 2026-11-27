using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase4SafetyTests
    {
        private GameObject _host;
        private ChemicalSelectionManager _sel;
        private ExperimentManager _exp;
        private SceneNavigationController _nav;

        [SetUp]
        public void SetUp()
        {
            ChemicalDatabase.ClearCache();
            _host = new GameObject("Phase4TestHost");
            _nav = _host.AddComponent<SceneNavigationController>();
            _sel = _host.AddComponent<ChemicalSelectionManager>();
            _exp = _host.AddComponent<ExperimentManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void HCl_Plus_NaOH_FailsClosed_InNormalMode()
        {
            _sel.IncludeSyntheticTestRecords = false;
            _sel.SelectPair("CH-02", "CH-05"); // HCl + NaOH (EXP-02 PendingReview)
            var result = _exp.AssessCurrentSelection();

            Assert.AreEqual(RiskCategory.Unknown, result.Risk, "Pending review HCl+NaOH must be UNKNOWN in normal mode.");
            Assert.IsFalse(result.IsAuthorizedByRule);
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution());
            Assert.IsFalse(_exp.CanEnterScene(X13SceneIds.ARLaboratory));
        }

        [Test]
        public void MissingAndUnreviewedPairs_BlockExecution()
        {
            _sel.IncludeSyntheticTestRecords = false;
            _sel.SelectPair("CH-04", "CH-06"); // Sulfuric + KOH (no scenario)
            var result = _exp.AssessCurrentSelection();

            Assert.AreEqual(RiskCategory.Unknown, result.Risk);
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution());
        }

        [Test]
        public void SyntheticLowRisk_PermitsSimulation()
        {
            _sel.IncludeSyntheticTestRecords = true;
            _sel.SelectPair("TEST-A", "TEST-B"); // TEST-01 (Low)
            var result = _exp.AssessCurrentSelection();

            Assert.AreEqual(RiskCategory.Low, result.Risk);
            Assert.IsTrue(_exp.VerifyAuthorizationForExecution());
            Assert.IsTrue(_exp.CanEnterScene(X13SceneIds.ARLaboratory));
        }

        [Test]
        public void SyntheticMediumRisk_RequiresScenarioBoundAcknowledgement_AndInvalidatesOnChange()
        {
            _sel.IncludeSyntheticTestRecords = true;
            _sel.SelectPair("TEST-A", "TEST-C"); // TEST-02 (Medium)
            var result = _exp.AssessCurrentSelection();

            Assert.AreEqual(RiskCategory.Medium, result.Risk);
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution(), "Medium must block before acknowledgement.");

            Assert.IsFalse(_exp.AcknowledgeMediumRisk("WRONG-ID", result.DataVersion));
            Assert.IsTrue(_exp.AcknowledgeMediumRisk(result.ScenarioId, result.DataVersion));
            Assert.IsTrue(_exp.VerifyAuthorizationForExecution());

            // Changing selection invalidates authorization
            _sel.SelectChemicalB("TEST-D");
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution());
        }

        [Test]
        public void SyntheticHighRisk_BlocksAndCannotBeAcknowledged()
        {
            _sel.IncludeSyntheticTestRecords = true;
            _sel.SelectPair("TEST-A", "TEST-D"); // TEST-03 (High)
            var result = _exp.AssessCurrentSelection();

            Assert.AreEqual(RiskCategory.High, result.Risk);
            Assert.IsFalse(_exp.AcknowledgeMediumRisk(result.ScenarioId, result.DataVersion));
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution());
            Assert.IsFalse(_exp.CanEnterScene(X13SceneIds.ARLaboratory));
        }
    }
}
