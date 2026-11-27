using UnityEngine;

namespace SectorX13.Data
{
    public enum ReviewStatus
    {
        PendingReview = 0,
        ReviewedApproved = 1,
        Rejected = 2,
        SyntheticTestOnly = 3
    }

    public enum RiskCategory
    {
        Unknown = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>
    /// Structured chemical profile matching the 6 information sections in the SECTOR X-13 specification.
    /// Catalogue membership does not imply permission to mix.
    /// </summary>
    [CreateAssetMenu(fileName = "CH_New", menuName = "SECTOR X-13/Chemical Data")]
    public class ChemicalData : ScriptableObject
    {
        [Header("1. Basic Info")]
        public string chemicalId;
        public string chemicalName;
        public string formula;
        public string category;

        [Header("2. Properties")]
        public string physicalForm;
        [TextArea(2, 4)] public string propertiesSummary;
        public string sourceReference;

        [Header("3. Safety Info")]
        public string sdsReference;
        [TextArea(2, 4)] public string hazardInfo;
        [TextArea(2, 4)] public string precautions;
        public ReviewStatus reviewStatus = ReviewStatus.PendingReview;

        [Header("4. Experiment Info")]
        public string[] supportedScenarioIds;
        [TextArea(2, 4)] public string fixedConditionsNote;

        [Header("5. AR Info")]
        [TextArea(2, 4)] public string arVisualDescription;
        public string visualAssetId;
        public Color liquidColor = new Color(0.10f, 0.72f, 0.88f, 0.85f);

        [Header("6. Risk Assessment Summary")]
        [TextArea(2, 4)] public string riskAssessmentSummary;

        [Header("Testing Isolation")]
        public bool isSyntheticTestRecord;

        public bool IsValidRecord()
        {
            return !string.IsNullOrWhiteSpace(chemicalId)
                && !string.IsNullOrWhiteSpace(chemicalName)
                && !string.IsNullOrWhiteSpace(formula);
        }
    }
}
