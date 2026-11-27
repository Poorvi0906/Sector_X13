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
                X13UIBuilderRuntime.CreateText(body, "CompleteCheck", "✓", 56, X13Theme.RiskLow, FontStyle.Bold, TextAnchor.MiddleCenter);
                X13UIBuilderRuntime.CreateText(body, "CompleteTag", "SIMULATION COMPLETE", 22, X13Theme.RiskLow, FontStyle.Bold, TextAnchor.MiddleCenter);
                X13UIBuilderRuntime.CreateText(body, "Title", exp.title, 36, X13Theme.TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
                X13UIBuilderRuntime.CreatePillBadge(body, "RiskPill", exp.reviewedRiskCategory.ToString().ToUpperInvariant(), SafetyAssessmentView.GetRiskColor(exp.reviewedRiskCategory));

                var eqCard = X13UIBuilderRuntime.CreateCard(body, "EquationCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(eqCard, "Tag", "BALANCED CHEMICAL EQUATION", 21, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);
                X13UIBuilderRuntime.CreateText(eqCard, "Equation", exp.equation, 30, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);

                var obsCard = X13UIBuilderRuntime.CreateCard(body, "ObsCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(obsCard, "Tag", "EXPECTED OBSERVATION", 21, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(obsCard, "Observations", exp.observations, 23, X13Theme.TextPrimary);

                var eduCard = X13UIBuilderRuntime.CreateCard(body, "EducationalCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(eduCard, "Tag", "SCIENTIFIC EXPLANATION & LIMITATIONS", 21, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(eduCard, "Outcome", exp.educationalOutcome, 23, X13Theme.TextPrimary);
                X13UIBuilderRuntime.CreateText(eduCard, "Safety", $"Safety Notes: {exp.safetyNotes}", 22, X13Theme.TextSecondary);
            }

            X13UIBuilderRuntime.CreateButton(
                body,
                "NewExperimentBtn",
                "RESTART EXPERIMENT",
                X13Theme.ButtonPrimaryBg,
                X13Theme.TextDark,
                () =>
                {
                    mgr.ResetSession();
                    navigate(X13SceneIds.ChemicalSelection);
                },
                92f,
                28,
                primaryGradient: true);

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
