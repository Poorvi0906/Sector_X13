using UnityEngine;

namespace SectorX13.Data
{
    /// <summary>
    /// Explicitly linked, independently reviewed alternative scenario record for Smart Safety Advisor.
    /// </summary>
    [CreateAssetMenu(fileName = "ALT_New", menuName = "SECTOR X-13/Alternative Link")]
    public class AlternativeLink : ScriptableObject
    {
        public string linkId;
        public string sourceScenarioId;
        public string sourceSelectionKey;
        public string targetScenarioId;
        [TextArea(2, 4)] public string reviewedRationale;
        public string reviewEvidence;
        public ReviewStatus reviewStatus = ReviewStatus.PendingReview;
        public bool isSyntheticTestRecord;

        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(linkId)
                && !string.IsNullOrWhiteSpace(targetScenarioId)
                && !string.IsNullOrWhiteSpace(reviewedRationale)
                && !string.IsNullOrWhiteSpace(reviewEvidence)
                && (!string.IsNullOrWhiteSpace(sourceScenarioId) || !string.IsNullOrWhiteSpace(sourceSelectionKey));
        }
    }
}
