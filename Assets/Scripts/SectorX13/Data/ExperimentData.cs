using UnityEngine;

namespace SectorX13.Data
{
    /// <summary>
    /// Predefined two-chemical scenario record with fixed conditions, review metadata,
    /// and authorized educational effect/result fields.
    /// </summary>
    [CreateAssetMenu(fileName = "EXP_New", menuName = "SECTOR X-13/Experiment Data")]
    public class ExperimentData : ScriptableObject
    {
        [Header("Scenario Identity")]
        public string scenarioId;
        public string title;
        public string chemicalAId;
        public string chemicalBId;

        [Header("Fixed Conditions (No Arbitrary Input)")]
        public string fixedConcentrationA;
        public string fixedQuantityA;
        public string fixedConcentrationB;
        public string fixedQuantityB;
        [TextArea(2, 3)] public string fixedConditions;

        [Header("Scientific Review & Risk")]
        public RiskCategory reviewedRiskCategory = RiskCategory.Unknown;
        public ReviewStatus reviewStatus = ReviewStatus.PendingReview;
        public string reviewEvidence;
        public string dataVersion = "0.0.0-pending";

        [Header("Safety Controls & Outcome")]
        public string[] controls;
        [TextArea(2, 4)] public string rationale;
        [TextArea(2, 4)] public string observations;
        public string equation;
        public string effectId;
        [TextArea(2, 4)] public string safetyNotes;
        [TextArea(2, 4)] public string educationalOutcome;

        [Header("Linked Alternatives")]
        public string[] alternativeIds;

        [Header("Testing Isolation")]
        public bool isSyntheticTestRecord;

        public bool MatchesPair(string idA, string idB)
        {
            if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB)) return false;
            return (chemicalAId == idA && chemicalBId == idB)
                || (chemicalAId == idB && chemicalBId == idA);
        }

        public bool IsCompleteForExecution()
        {
            return !string.IsNullOrWhiteSpace(scenarioId)
                && !string.IsNullOrWhiteSpace(chemicalAId)
                && !string.IsNullOrWhiteSpace(chemicalBId)
                && !string.IsNullOrWhiteSpace(fixedQuantityA)
                && !string.IsNullOrWhiteSpace(fixedQuantityB)
                && !string.IsNullOrWhiteSpace(dataVersion)
                && !string.IsNullOrWhiteSpace(effectId);
        }
    }
}
