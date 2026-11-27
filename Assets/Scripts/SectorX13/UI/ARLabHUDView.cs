using SectorX13.Core;
using SectorX13.Lab;
using UnityEngine;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders the 07_AR_Laboratory HUD and binds POUR / MIX / RESET / VIEW RESULTS
    /// directly to ReactionController and ExperimentManager.
    /// </summary>
    public static class ARLabHUDView
    {
        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            var mgr = ExperimentManager.Instance;
            var assessment = mgr.CurrentAssessment;
            string subtitle = assessment != null ? $"{assessment.ScenarioId} · {assessment.Risk}" : "UNAUTHORIZED";

            X13UIBuilderRuntime.CreateHeader(root, "AR VIRTUAL LAB", subtitle, Application.isEditor ? "EDITOR SIM" : "AR LIVE", true, goBack);

            var statusWrap = new GameObject("SimStatusWrap", typeof(RectTransform));
            statusWrap.transform.SetParent(root, false);
            var swRt = statusWrap.GetComponent<RectTransform>();
            swRt.anchorMin = new Vector2(0.08f, 0.77f);
            swRt.anchorMax = new Vector2(0.92f, 0.87f);
            swRt.offsetMin = Vector2.zero;
            swRt.offsetMax = Vector2.zero;

            var statusCard = X13UIBuilderRuntime.CreateCard(statusWrap.transform, "SimStatusCard", X13Theme.CardSurface, X13Theme.CyanAccent);
            statusCard.anchorMin = Vector2.zero;
            statusCard.anchorMax = Vector2.one;
            statusCard.offsetMin = Vector2.zero;
            statusCard.offsetMax = Vector2.zero;
            var statusLabel = X13UIBuilderRuntime.CreateText(statusCard, "Label", "SIMULATION STATUS: READY", 24, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);

            var lab = Object.FindFirstObjectByType<ReactionController>();

            var controlsWrap = new GameObject("LabControls", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            controlsWrap.transform.SetParent(root, false);
            var cwRt = controlsWrap.GetComponent<RectTransform>();
            cwRt.anchorMin = new Vector2(0.04f, 0.03f);
            cwRt.anchorMax = new Vector2(0.96f, 0.13f);
            cwRt.offsetMin = Vector2.zero;
            cwRt.offsetMax = Vector2.zero;
            var hlg = controlsWrap.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            var pourBtn = X13UIBuilderRuntime.CreateButton(controlsWrap.transform, "PourBtn", "POUR", X13Theme.ButtonSecondaryBg, X13Theme.CyanAccent, () => lab?.TriggerPour(), 92f, 26);
            var mixBtn = X13UIBuilderRuntime.CreateButton(controlsWrap.transform, "MixBtn", "MIX", X13Theme.ButtonSecondaryBg, X13Theme.CyanAccent, () => lab?.TriggerMix(), 92f, 26);
            var resultsBtn = X13UIBuilderRuntime.CreateButton(controlsWrap.transform, "ResultsBtn", "RESULTS ->", X13Theme.ButtonDisabledBg, X13Theme.TextSecondary, () =>
            {
                if (mgr.HasCompletedScenario) navigate(X13SceneIds.ExperimentResults);
            }, 92f, 24);
            X13UIBuilderRuntime.CreateButton(controlsWrap.transform, "ResetBtn", "RESET", X13Theme.ButtonSecondaryBg, X13Theme.TextPrimary, () =>
            {
                lab?.ResetVisuals();
                mgr.ResetSession();
                navigate(X13SceneIds.Home);
            }, 92f, 24);

            void RefreshHUD()
            {
                bool authorized = mgr.VerifyAuthorizationForExecution();
                if (lab != null)
                {
                    statusLabel.text = authorized ? lab.StatusMessage : "BLOCKED: RETURN TO ASSESSMENT";
                    pourBtn.interactable = authorized && !lab.IsSimulating && !lab.HasPoured;
                    mixBtn.interactable = authorized && !lab.IsSimulating && !lab.HasMixed;
                }
                else
                {
                    statusLabel.text = authorized ? "READY (NO 3D RIG)" : "BLOCKED: UNAUTHORIZED";
                    pourBtn.interactable = false;
                    mixBtn.interactable = false;
                }

                bool done = mgr.HasCompletedScenario;
                resultsBtn.interactable = done;
                resultsBtn.GetComponent<Image>().color = done ? X13Theme.ButtonPrimaryBg : X13Theme.ButtonDisabledBg;
                resultsBtn.GetComponentInChildren<Text>().color = done ? X13Theme.TextDark : X13Theme.TextSecondary;
            }

            if (lab != null) lab.OnLabStateChanged += RefreshHUD;
            RefreshHUD();
        }
    }
}
