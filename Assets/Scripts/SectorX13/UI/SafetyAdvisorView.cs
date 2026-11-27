using System.Collections.Generic;
using SectorX13.Core;
using UnityEngine;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders 06_Safety_Advisor showing the current restriction, eligible reviewed alternatives,
    /// or a clear "no alternative available" state while keeping the original blocked.
    /// </summary>
    public static class SafetyAdvisorView
    {
        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            X13UIBuilderRuntime.CreateHeader(root, "SMART SAFETY ADVISOR", "REVIEWED ALTERNATIVES", Application.isEditor ? "EDITOR SIM" : "ONLINE", true, goBack);
            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            var sel = ChemicalSelectionManager.Instance;
            var mgr = ExperimentManager.Instance;
            var assessment = mgr.CurrentAssessment;

            var restrictionCard = X13UIBuilderRuntime.CreateCard(body, "RestrictionCard", X13Theme.CardSurface, X13Theme.RiskHigh);
            X13UIBuilderRuntime.CreateText(restrictionCard, "Tag", "CURRENT RESTRICTION", 22, X13Theme.RiskHigh, FontStyle.Bold);
            string pairText = sel.HasBothSelections
                ? $"{sel.GetChemicalA()?.chemicalName ?? sel.ChemicalAId} + {sel.GetChemicalB()?.chemicalName ?? sel.ChemicalBId}"
                : "No reagent pair selected";
            X13UIBuilderRuntime.CreateText(restrictionCard, "Pair", pairText, 27, X13Theme.TextPrimary, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(
                restrictionCard,
                "Reason",
                assessment != null ? $"{assessment.StatusTitle}: {assessment.Rationale}" : "Mixing is blocked until a reviewed scenario is assessed.",
                23,
                X13Theme.TextSecondary);

            var db = Data.ChemicalDatabase.Load();
            List<AdvisorSuggestion> suggestions = SmartSafetyAdvisor.GetEligibleAlternatives(
                db,
                assessment?.ScenarioId,
                sel.ChemicalAId,
                sel.ChemicalBId,
                sel.IncludeSyntheticTestRecords);

            if (suggestions.Count == 0)
            {
                var emptyCard = X13UIBuilderRuntime.CreateCard(body, "NoAltCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(emptyCard, "Title", "No Reviewed Alternative Available", 28, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(emptyCard, "Body", "No independently reviewed, explicitly linked alternative exists for this selection. The original combination remains blocked.", 23, X13Theme.TextSecondary);
            }
            else
            {
                foreach (var s in suggestions)
                {
                    var captured = s;
                    var altCard = X13UIBuilderRuntime.CreateCard(body, $"Alt_{captured.Link.linkId}", X13Theme.CardSurface, X13Theme.CyanAccent);
                    X13UIBuilderRuntime.CreateText(altCard, "Title", $"ALTERNATIVE: {captured.TargetChemicalA.chemicalName} + {captured.TargetChemicalB.chemicalName}", 26, X13Theme.CyanAccent, FontStyle.Bold);
                    X13UIBuilderRuntime.CreateText(altCard, "Meta", $"Target: {captured.TargetExperiment.scenarioId} · Risk: {captured.TargetExperiment.reviewedRiskCategory} · Review: {captured.Link.reviewStatus}", 22, X13Theme.TextPrimary, FontStyle.Bold);
                    X13UIBuilderRuntime.CreateText(altCard, "Rationale", $"Rationale: {captured.Link.reviewedRationale}\nFixed Conditions: {captured.TargetExperiment.fixedConditions}", 22, X13Theme.TextSecondary);
                    X13UIBuilderRuntime.CreateButton(
                        altCard,
                        $"SelectAlt_{captured.Link.linkId}",
                        "SELECT & REASSESS FROM SCRATCH  ->",
                        X13Theme.ButtonPrimaryBg,
                        X13Theme.TextDark,
                        () =>
                        {
                            if (SmartSafetyAdvisor.ApplyAlternative(captured))
                            {
                                navigate(X13SceneIds.SafetyAssessment);
                            }
                        },
                        84f,
                        24);
                }
            }

            X13UIBuilderRuntime.CreateButton(body, "ReturnAssessmentBtn", "RETURN TO ASSESSMENT", X13Theme.ButtonSecondaryBg, X13Theme.CyanAccent, () => navigate(X13SceneIds.SafetyAssessment), 84f, 26);
            X13UIBuilderRuntime.CreateButton(body, "ReselectBtn", "RESELECT CHEMICALS", X13Theme.ButtonSecondaryBg, X13Theme.TextPrimary, () => navigate(X13SceneIds.ChemicalSelection), 84f, 26);

            X13UIBuilderRuntime.CreateBottomNav(root, 3, navigate);
        }
    }
}
