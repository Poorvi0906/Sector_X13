using System;
using System.Collections.Generic;
using SectorX13.Data;

namespace SectorX13.Core
{
    [Serializable]
    public class AdvisorSuggestion
    {
        public AlternativeLink Link;
        public ExperimentData TargetExperiment;
        public ChemicalData TargetChemicalA;
        public ChemicalData TargetChemicalB;
    }

    /// <summary>
    /// Retrieves only explicitly linked, independently reviewed, complete safer alternatives.
    /// Never invents alternatives or assumes dilution alone makes a pair safe.
    /// </summary>
    public static class SmartSafetyAdvisor
    {
        public static List<AdvisorSuggestion> GetEligibleAlternatives(
            ChemicalDatabase db,
            string sourceScenarioId,
            string chemicalAId,
            string chemicalBId,
            bool allowSyntheticTestMode = false)
        {
            var results = new List<AdvisorSuggestion>();
            if (db == null || db.alternatives == null) return results;

            string pairKey = ChemicalDatabase.BuildSelectionKey(chemicalAId, chemicalBId);

            foreach (var link in db.alternatives)
            {
                if (link == null || !link.IsComplete()) continue;

                bool linkReviewValid = link.reviewStatus == ReviewStatus.ReviewedApproved
                    || (allowSyntheticTestMode && link.isSyntheticTestRecord && link.reviewStatus == ReviewStatus.SyntheticTestOnly);
                if (!linkReviewValid) continue;

                bool matchesSource = (!string.IsNullOrEmpty(sourceScenarioId) && link.sourceScenarioId == sourceScenarioId)
                    || (!string.IsNullOrEmpty(pairKey) && link.sourceSelectionKey == pairKey);
                if (!matchesSource) continue;

                var targetExp = db.GetExperimentById(link.targetScenarioId);
                if (targetExp == null || !targetExp.IsCompleteForExecution()) continue;

                bool targetReviewValid = targetExp.reviewStatus == ReviewStatus.ReviewedApproved
                    || (allowSyntheticTestMode && targetExp.isSyntheticTestRecord && targetExp.reviewStatus == ReviewStatus.SyntheticTestOnly);
                if (!targetReviewValid) continue;

                if (targetExp.reviewedRiskCategory != RiskCategory.Low && targetExp.reviewedRiskCategory != RiskCategory.Medium)
                {
                    continue;
                }

                var chemA = db.GetChemicalById(targetExp.chemicalAId);
                var chemB = db.GetChemicalById(targetExp.chemicalBId);
                if (chemA == null || chemB == null) continue;

                results.Add(new AdvisorSuggestion
                {
                    Link = link,
                    TargetExperiment = targetExp,
                    TargetChemicalA = chemA,
                    TargetChemicalB = chemB
                });
            }

            return results;
        }

        /// <summary>
        /// Applies a selected alternative by clearing prior authorization and updating selection
        /// so SafetyAssessment evaluates the alternative from scratch.
        /// </summary>
        public static bool ApplyAlternative(AdvisorSuggestion suggestion)
        {
            if (suggestion == null || suggestion.TargetExperiment == null) return false;
            ExperimentManager.Instance.InvalidateAuthorization();
            ChemicalSelectionManager.Instance.SelectPair(
                suggestion.TargetExperiment.chemicalAId,
                suggestion.TargetExperiment.chemicalBId);
            return true;
        }
    }
}
