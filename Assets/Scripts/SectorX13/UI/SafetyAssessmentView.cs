using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders 05_Safety_Assessment with fail-closed risk banner (text + color),
    /// scenario details, medium-risk acknowledgement gate, and advisor/reselection routing.
    /// </summary>
    public static class SafetyAssessmentView
    {
        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            var mgr = ExperimentManager.Instance;
            var result = mgr.AssessCurrentSelection();

            X13UIBuilderRuntime.CreateHeader(
                root,
                "SAFETY ASSESSMENT",
                string.IsNullOrEmpty(result.ScenarioId) ? "FAIL-CLOSED GATE" : $"{result.ScenarioId} · v{result.DataVersion}",
                Application.isEditor ? "EDITOR SIM" : "ONLINE",
                true,
                goBack);

            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            Color riskColor = GetRiskColor(result.Risk);
            var banner = X13UIBuilderRuntime.CreateCard(body, "RiskBanner", X13Theme.CardSurface, riskColor);
            var atomRow = X13UIBuilderRuntime.CreateRow(banner, "AtomRow");
            X13UIBuilderRuntime.CreateAtomIcon(atomRow, "SafetyAtom", 112f);
            X13UIBuilderRuntime.CreatePillBadge(banner, "RiskPill", result.Risk.ToString().ToUpperInvariant(), riskColor);
            X13UIBuilderRuntime.CreateText(banner, "RiskBadge", result.StatusTitle, 28, riskColor, FontStyle.Bold, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(banner, "ReviewStatus", $"Review Status: {result.ReviewStatus}", 21, X13Theme.TextSecondary, FontStyle.Bold, TextAnchor.MiddleCenter);

            var sel = ChemicalSelectionManager.Instance;
            var chemA = sel.GetChemicalA();
            var chemB = sel.GetChemicalB();
            var pairRow = X13UIBuilderRuntime.CreateRow(body, "PairRow", 16f);
            var cardA = X13UIBuilderRuntime.CreateCard(pairRow, "ReactantACard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(cardA, "TagA", "REACTANT A", 20, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(cardA, "ValA", chemA != null ? $"{chemA.formula}\n<size=21><color=#8FA8B8>{chemA.chemicalName}</color></size>" : "Not selected", 30, X13Theme.TextPrimary, FontStyle.Bold);

            var cardB = X13UIBuilderRuntime.CreateCard(pairRow, "ReactantBCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(cardB, "TagB", "REACTANT B", 20, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(cardB, "ValB", chemB != null ? $"{chemB.formula}\n<size=21><color=#8FA8B8>{chemB.chemicalName}</color></size>" : "Not selected", 30, X13Theme.TextPrimary, FontStyle.Bold);

            var presetCard = X13UIBuilderRuntime.CreateCard(body, "PresetCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(presetCard, "Tag", "EXPERIMENT PRESET", 20, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(presetCard, "Cond", result.FixedConditions, 23, X13Theme.TextPrimary);

            var rationaleCard = X13UIBuilderRuntime.CreateCard(body, "RationaleCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(rationaleCard, "Heading", "SAFETY RATIONALE & CONTROLS", 22, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(rationaleCard, "Rationale", result.Rationale, 24, X13Theme.TextPrimary);
            if (result.Controls != null && result.Controls.Length > 0)
            {
                X13UIBuilderRuntime.CreateText(rationaleCard, "Controls", "Controls: " + string.Join(" · ", result.Controls), 22, X13Theme.TextSecondary);
            }

            Button launchBtn = null;
            if (result.Risk == RiskCategory.Medium && result.RequiresAcknowledgement)
            {
                bool ackChecked = false;
                var ackCard = X13UIBuilderRuntime.CreateCard(body, "AckCard", X13Theme.CardElevated, X13Theme.RiskMedium);
                var ackBtn = X13UIBuilderRuntime.CreateButton(
                    ackCard,
                    "AckToggleBtn",
                    "[ ] I understand this is a virtual-only preset and acknowledge the controls",
                    X13Theme.CardSurface,
                    X13Theme.TextPrimary,
                    null,
                    92f,
                    22);

                launchBtn = X13UIBuilderRuntime.CreateButton(
                    body,
                    "LaunchARBtn",
                    "ACKNOWLEDGE TO LAUNCH AR SIMULATION",
                    X13Theme.ButtonDisabledBg,
                    X13Theme.TextSecondary,
                    () =>
                    {
                        if (mgr.VerifyAuthorizationForExecution())
                        {
                            navigate(X13SceneIds.ARLaboratory);
                        }
                    },
                    96f,
                    28);
                launchBtn.interactable = false;

                ackBtn.onClick.AddListener(() =>
                {
                    if (!ackChecked && mgr.AcknowledgeMediumRisk(result.ScenarioId, result.DataVersion))
                    {
                        ackChecked = true;
                        ackBtn.GetComponentInChildren<Text>().text = "[X] ACKNOWLEDGED FOR " + result.ScenarioId;
                        ackBtn.GetComponent<Image>().color = X13Theme.CardElevated;
                        launchBtn.interactable = true;
                        launchBtn.GetComponent<Image>().color = X13Theme.ButtonPrimaryBg;
                        launchBtn.GetComponentInChildren<Text>().color = X13Theme.TextDark;
                        launchBtn.GetComponentInChildren<Text>().text = "LAUNCH AR SIMULATION  ->";
                    }
                });
            }
            else if (result.Risk == RiskCategory.Low && mgr.VerifyAuthorizationForExecution())
            {
                X13UIBuilderRuntime.CreateButton(
                    body,
                    "LaunchARBtn",
                    "LAUNCH AR SIMULATION  ->",
                    X13Theme.ButtonPrimaryBg,
                    X13Theme.TextDark,
                    () => navigate(X13SceneIds.ARLaboratory),
                    96f,
                    28);
            }

            if (result.Risk == RiskCategory.High || result.Risk == RiskCategory.Unknown)
            {
                X13UIBuilderRuntime.CreateButton(
                    body,
                    "OpenAdvisorBtn",
                    "OPEN SMART SAFETY ADVISOR  ->",
                    X13Theme.ButtonSecondaryBg,
                    X13Theme.CyanAccent,
                    () => navigate(X13SceneIds.SafetyAdvisor),
                    88f,
                    26);
            }

            X13UIBuilderRuntime.CreateButton(
                body,
                "ReselectBtn",
                "RESELECT CHEMICALS",
                X13Theme.ButtonSecondaryBg,
                X13Theme.TextPrimary,
                () => navigate(X13SceneIds.ChemicalSelection),
                84f,
                25);

            X13UIBuilderRuntime.CreateBottomNav(root, 3, navigate);
        }

        public static Color GetRiskColor(RiskCategory risk)
        {
            switch (risk)
            {
                case RiskCategory.Low: return X13Theme.RiskLow;
                case RiskCategory.Medium: return X13Theme.RiskMedium;
                case RiskCategory.High: return X13Theme.RiskHigh;
                default: return X13Theme.RiskUnknown;
            }
        }
    }
}
