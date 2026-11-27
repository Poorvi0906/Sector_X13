using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase7IntegrationTests
    {
        private GameObject _host;
        private ChemicalSelectionManager _sel;
        private ExperimentManager _exp;
        private SceneNavigationController _nav;

        [SetUp]
        public void SetUp()
        {
            ChemicalDatabase.ClearCache();
            _host = new GameObject("Phase7TestHost");
            _nav = _host.AddComponent<SceneNavigationController>();
            _sel = _host.AddComponent<ChemicalSelectionManager>();
            _exp = _host.AddComponent<ExperimentManager>();
            _exp.EnsureInitialized();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void DirectNavigationToLabOrResults_IsBlockedWithoutAuthorization()
        {
            Assert.IsFalse(_nav.NavigateTo(X13SceneIds.ARLaboratory));
            Assert.IsFalse(_nav.NavigateTo(X13SceneIds.ExperimentResults));
        }

        [Test]
        public void FullEndToEndLifecycle_AndReset_WorkDeterministically()
        {
            _sel.IncludeSyntheticTestRecords = true;
            Assert.AreEqual(ExperimentLifecycleState.Idle, _exp.State);

            _sel.SelectPair("TEST-A", "TEST-C"); // TEST-02 Medium
            Assert.AreEqual(ExperimentLifecycleState.Selected, _exp.State);

            var res = _exp.AssessCurrentSelection();
            Assert.AreEqual(RiskCategory.Medium, res.Risk);
            Assert.AreEqual(ExperimentLifecycleState.Prohibited, _exp.State);

            Assert.IsTrue(_exp.AcknowledgeMediumRisk(res.ScenarioId, res.DataVersion));
            Assert.AreEqual(ExperimentLifecycleState.Authorized, _exp.State);

            Assert.IsTrue(_exp.BeginSimulation());
            Assert.AreEqual(ExperimentLifecycleState.Simulating, _exp.State);

            Assert.IsTrue(_exp.CompleteSimulation());
            Assert.AreEqual(ExperimentLifecycleState.Completed, _exp.State);
            Assert.IsTrue(_exp.HasCompletedScenario);
            Assert.IsTrue(_exp.CanEnterScene(X13SceneIds.ExperimentResults));

            _exp.ResetSession();
            Assert.AreEqual(ExperimentLifecycleState.Idle, _exp.State);
            Assert.IsFalse(_sel.HasBothSelections);
            Assert.IsFalse(_exp.HasCompletedScenario);
            Assert.IsFalse(_exp.VerifyAuthorizationForExecution());
            Assert.IsFalse(_exp.CanEnterScene(X13SceneIds.ARLaboratory));
            Assert.IsFalse(_exp.CanEnterScene(X13SceneIds.ExperimentResults));
        }
    }
}
