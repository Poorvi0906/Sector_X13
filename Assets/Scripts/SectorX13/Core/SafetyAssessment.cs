using System;
using System.Collections.Generic;
using SectorX13.Data;

namespace SectorX13.Core
{
    [Serializable]
    public class SafetyAssessmentResult
    {
        public string ScenarioId;
        public string DataVersion;
        public string ChemicalAId;
        public string ChemicalBId;
        public RiskCategory Risk = RiskCategory.Unknown;
        public ReviewStatus ReviewStatus = ReviewStatus.PendingReview;
        public string StatusTitle = "NOT VERIFIED";
        public string Rationale = "No verified scenario record.";
        public string FixedConditions = "None";
        public string[] Controls = Array.Empty<string>();
        public bool RequiresAcknowledgement;
        public bool IsAuthorizedByRule;
        public bool IsSyntheticTestRecord;
        public ExperimentData BoundExperiment;
    }

    /// <summary>
    /// Fail-closed safety assessment engine.
    /// Never infers risk from chemical names or unvalidated numeric scores.
    /// </summary>
    public static class SafetyAssessment
    {
        public static SafetyAssessmentResult Evaluate(
            ChemicalDatabase db,
            string chemicalAId,
            string chemicalBId,
            bool allowSyntheticTestMode = false)
        {
            var result = new SafetyAssessmentResult
            {
                ChemicalAId = chemicalAId,
                ChemicalBId = chemicalBId,
                Risk = RiskCategory.Unknown,
                ReviewStatus = ReviewStatus.PendingReview,
                StatusTitle = "NOT VERIFIED",
                IsAuthorizedByRule = false,
                RequiresAcknowledgement = false
            };

            if (db == null)
            {
                result.Rationale = "Chemical database unavailable. Mixing blocked.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(chemicalAId) || string.IsNullOrWhiteSpace(chemicalBId))
            {
                result.Rationale = "Two explicit chemical selections are required. Mixing blocked.";
                return result;
            }

            if (chemicalAId == chemicalBId)
            {
                result.Rationale = "Two distinct reagents must be selected. Mixing blocked.";
                return result;
            }

            List<ExperimentData> matches = db.FindExperimentsForPair(chemicalAId, chemicalBId, allowSyntheticTestMode);
            if (matches.Count == 0)
            {
                result.Rationale = "No predefined scenario record exists for this reagent pair. Mixing blocked.";
                return result;
            }

            if (matches.Count > 1)
            {
                result.Rationale = "Ambiguous scenario records detected for this pair. Fail-closed: mixing blocked.";
                return result;
            }

            ExperimentData exp = matches[0];
            result.ScenarioId = exp.scenarioId;
            result.DataVersion = exp.dataVersion;
            result.ReviewStatus = exp.reviewStatus;
            result.FixedConditions = string.IsNullOrWhiteSpace(exp.fixedConditions) ? "Unspecified" : exp.fixedConditions;
            result.Controls = exp.controls ?? Array.Empty<string>();
            result.IsSyntheticTestRecord = exp.isSyntheticTestRecord;
            result.BoundExperiment = exp;

            if (!exp.IsCompleteForExecution())
            {
                result.Risk = RiskCategory.Unknown;
                result.StatusTitle = "NOT VERIFIED · INCOMPLETE";
                result.Rationale = "Scenario record is incomplete (missing fixed quantities, version, or effect ID). Mixing blocked.";
                return result;
            }

            bool reviewValid = exp.reviewStatus == ReviewStatus.ReviewedApproved
                || (allowSyntheticTestMode && exp.isSyntheticTestRecord && exp.reviewStatus == ReviewStatus.SyntheticTestOnly);

            if (!reviewValid)
            {
                result.Risk = RiskCategory.Unknown;
                result.StatusTitle = "NOT VERIFIED · PENDING REVIEW";
                result.Rationale = string.IsNullOrWhiteSpace(exp.rationale)
                    ? "Scenario awaits scientific review. Normal mode blocks unreviewed records."
                    : exp.rationale;
                return result;
            }

            result.Risk = exp.reviewedRiskCategory;
            result.Rationale = string.IsNullOrWhiteSpace(exp.rationale) ? "Reviewed protocol." : exp.rationale;

            switch (exp.reviewedRiskCategory)
            {
                case RiskCategory.Low:
                    result.StatusTitle = "LOW RISK · PERMITTED";
                    result.IsAuthorizedByRule = true;
                    result.RequiresAcknowledgement = false;
                    break;
                case RiskCategory.Medium:
                    result.StatusTitle = "MEDIUM RISK · ACKNOWLEDGEMENT REQUIRED";
                    result.IsAuthorizedByRule = false;
                    result.RequiresAcknowledgement = true;
                    break;
                case RiskCategory.High:
                    result.StatusTitle = "DANGER / UNSAFE · BLOCKED";
                    result.IsAuthorizedByRule = false;
                    result.RequiresAcknowledgement = false;
                    break;
                default:
                    result.Risk = RiskCategory.Unknown;
                    result.StatusTitle = "NOT VERIFIED · BLOCKED";
                    result.IsAuthorizedByRule = false;
                    result.RequiresAcknowledgement = false;
                    break;
            }

            return result;
        }
    }
}
