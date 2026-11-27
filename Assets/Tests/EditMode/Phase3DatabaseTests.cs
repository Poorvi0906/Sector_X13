using NUnit.Framework;
using SectorX13.Core;
using SectorX13.Data;
using SectorX13.UI;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase3DatabaseTests
    {
        [Test]
        public void NormalCatalogue_ContainsExactTenChemicalsAndExcludesSynthetic()
        {
            ChemicalDatabase.ClearCache();
            var db = ChemicalDatabase.Load();
            Assert.IsNotNull(db, "X13Database.asset must exist under Resources/X13Data/");

            var catalogue = db.GetCatalogue(includeSynthetic: false);
            Assert.AreEqual(10, catalogue.Count, "Normal catalogue must contain CH-01 through CH-10.");

            foreach (var c in catalogue)
            {
                Assert.IsFalse(c.isSyntheticTestRecord, $"Synthetic record {c.chemicalId} leaked into normal catalogue.");
                Assert.IsTrue(c.IsValidRecord(), $"Chemical {c.chemicalId} has incomplete basic fields.");
                for (int s = 0; s < 6; s++)
                {
                    string content = ChemicalLibraryView.GetSectionContent(c, s);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(content), $"Section {s} empty for {c.chemicalId}");
                }
            }
        }

        [Test]
        public void RealExperimentCandidates_ArePendingAndFailClosed()
        {
            var db = ChemicalDatabase.Load();
            Assert.IsNotNull(db);

            string[] candidates = { "EXP-01", "EXP-02", "EXP-03" };
            foreach (var id in candidates)
            {
                var exp = db.GetExperimentById(id);
                Assert.IsNotNull(exp, $"Missing candidate {id}");
                Assert.AreEqual(ReviewStatus.PendingReview, exp.reviewStatus, $"{id} must remain PendingReview until scientifically approved.");
                Assert.AreEqual(RiskCategory.Unknown, exp.reviewedRiskCategory, $"{id} must remain RiskCategory.Unknown until approved.");
            }
        }

        [Test]
        public void SelectionManager_StartsUnselectedAndClearsCleanly()
        {
            var go = new GameObject("TestSelMgr");
            var mgr = go.AddComponent<ChemicalSelectionManager>();
            Assert.IsFalse(mgr.HasBothSelections, "Selection must not default to index 0 for both slots.");
            Assert.IsNull(mgr.ChemicalAId);
            Assert.IsNull(mgr.ChemicalBId);

            mgr.SelectChemicalA("CH-02");
            Assert.IsFalse(mgr.HasBothSelections);
            mgr.SelectChemicalB("CH-05");
            Assert.IsTrue(mgr.HasBothSelections);

            mgr.ClearSelection();
            Assert.IsFalse(mgr.HasBothSelections);
            Object.DestroyImmediate(go);
        }
    }
}
