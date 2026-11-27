using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using SectorX13.Lab;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase6LabTests
    {
        private GameObject _host;
        private ChemicalSelectionManager _sel;
        private ExperimentManager _exp;
        private ReactionController _lab;

        [SetUp]
        public void SetUp()
        {
            ChemicalDatabase.ClearCache();
            _host = new GameObject("Phase6TestHost");
            _host.AddComponent<SceneNavigationController>();
            _sel = _host.AddComponent<ChemicalSelectionManager>();
            _exp = _host.AddComponent<ExperimentManager>();
            _lab = _host.AddComponent<ReactionController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void UnauthorizedLab_BlocksPourAndMix()
        {
            _sel.IncludeSyntheticTestRecords = false;
            _sel.SelectPair("CH-02", "CH-05"); // PendingReview -> blocked
            _exp.AssessCurrentSelection();

            Assert.IsFalse(_lab.TriggerPour());
            Assert.IsFalse(_lab.TriggerMix());
            Assert.IsFalse(_exp.CanEnterScene(X13SceneIds.ExperimentResults));
        }

        [Test]
        public void AuthorizedLowRisk_AllowsSimulationAndCompletionGate()
        {
            _sel.IncludeSyntheticTestRecords = true;
            _sel.SelectPair("TEST-A", "TEST-B"); // TEST-01 (Low)
            _exp.AssessCurrentSelection();

            Assert.IsTrue(_exp.BeginSimulation());
            Assert.IsTrue(_exp.CompleteSimulation());
            Assert.IsTrue(_exp.HasCompletedScenario);
            Assert.IsTrue(_exp.CanEnterScene(X13SceneIds.ExperimentResults));
        }
    }
}
