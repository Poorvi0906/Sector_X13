using SectorX13.Core;
using UnityEngine;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders 08_Experiment_Results for a completed, authorized scenario.
    /// Never fabricates equations, observations, or measurements.
    /// </summary>
    public static class ExperimentResultsView
    {
        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            var mgr = ExperimentManager.Instance;
            bool valid = mgr.HasCompletedScenario && mgr.VerifyAuthorizationForExecution();
            var exp = valid ? mgr.CurrentAssessment.BoundExperiment : null;

            X13UIBuilderRuntime.CreateHeader(
                root,
                "EXPERIMENT RESULTS",
                exp != null ? $"{exp.scenarioId} · COMPLETED" : "NO COMPLETED SCENARIO",
                Application.isEditor ? "EDITOR SIM" : "ONLINE",
                true,
                goBack);

            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            if (exp == null)
            {
                var emptyCard = X13UIBuilderRuntime.CreateCard(body, "EmptyResults", X13Theme.CardSurface, X13Theme.RiskUnknown);
                X13UIBuilderRuntime.CreateText(emptyCard, "Title", "No Completed Scenario", 30, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(emptyCard, "Body", "Results are shown only after an authorized virtual simulation completes. No fabricated equation or chemical measurement is displayed.", 24, X13Theme.TextSecondary);
            }
            else
            {
                var summaryCard = X13UIBuilderRuntime.CreateCard(body, "SummaryCard", X13Theme.CardSurface, X13Theme.CyanAccent);
                X13UIBuilderRuntime.CreateText(summaryCard, "Title", exp.title, 28, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(summaryCard, "Meta", $"Scenario: {exp.scenarioId} · Risk: {exp.reviewedRiskCategory} · Review: {exp.reviewStatus}", 22, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(summaryCard, "Conditions", $"Fixed Conditions: {exp.fixedConditions}", 22, X13Theme.TextSecondary);

                var eqCard = X13UIBuilderRuntime.CreateCard(body, "EquationCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(eqCard, "Tag", "EQUATION & OBSERVATIONS (VIRTUAL MODEL)", 22, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(eqCard, "Equation", exp.equation, 26, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(eqCard, "Observations", exp.observations, 23, X13Theme.TextSecondary);

                var eduCard = X13UIBuilderRuntime.CreateCard(body, "EducationalCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(eduCard, "Tag", "SAFETY NOTES & EDUCATIONAL OUTCOME", 22, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(eduCard, "Safety", $"Safety Notes: {exp.safetyNotes}", 23, X13Theme.TextPrimary);
                X13UIBuilderRuntime.CreateText(eduCard, "Outcome", $"Outcome: {exp.educationalOutcome}", 23, X13Theme.TextSecondary);
            }

            X13UIBuilderRuntime.CreateButton(
                body,
                "NewExperimentBtn",
                "RESET & NEW EXPERIMENT  ->",
                X13Theme.ButtonPrimaryBg,
                X13Theme.TextDark,
                () =>
                {
                    mgr.ResetSession();
                    navigate(X13SceneIds.ChemicalSelection);
                },
                92f,
                28);

            X13UIBuilderRuntime.CreateButton(
                body,
                "ReturnHomeBtn",
                "RESET & RETURN HOME",
                X13Theme.ButtonSecondaryBg,
                X13Theme.CyanAccent,
                () =>
                {
                    mgr.ResetSession();
                    SceneNavigationController.Instance.GoHome(clearHistory: true);
                },
                88f,
                26);

            X13UIBuilderRuntime.CreateBottomNav(root, 0, navigate);
        }
    }
}
