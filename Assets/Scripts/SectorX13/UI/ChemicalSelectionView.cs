using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders deliberate two-beaker reagent selection and fixed scenario preview for 04_Chemical_Selection.
    /// </summary>
    public static class ChemicalSelectionView
    {
        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            X13UIBuilderRuntime.CreateHeader(root, "SELECT REACTANTS", "CHOOSE EXACTLY TWO", Application.isEditor ? "EDITOR SIM" : "ONLINE", true, goBack);
            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            var db = ChemicalDatabase.Load();
            var sel = ChemicalSelectionManager.Instance;

            var slotsCard = X13UIBuilderRuntime.CreateCard(body, "SlotsCard", X13Theme.CardSurface, X13Theme.CyanAccent);
            var slotAText = X13UIBuilderRuntime.CreateText(slotsCard, "SlotA", "", 28, X13Theme.TextPrimary, FontStyle.Bold);
            var slotBText = X13UIBuilderRuntime.CreateText(slotsCard, "SlotB", "", 28, X13Theme.TextPrimary, FontStyle.Bold);
            var scenarioPreview = X13UIBuilderRuntime.CreateText(slotsCard, "ScenarioPreview", "", 23, X13Theme.TextSecondary);

            bool selectingSlotA = true;

            var modeRow = new GameObject("SlotToggleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            modeRow.transform.SetParent(slotsCard, false);
            var hlg = modeRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            var targetABtn = X13UIBuilderRuntime.CreateButton(modeRow.transform, "TargetABtn", "ASSIGNING: SLOT A", X13Theme.CardElevated, X13Theme.CyanAccent, null, 72f, 22);
            var targetBBtn = X13UIBuilderRuntime.CreateButton(modeRow.transform, "TargetBBtn", "ASSIGN SLOT B", X13Theme.ButtonSecondaryBg, X13Theme.TextSecondary, null, 72f, 22);
            var clearBtn = X13UIBuilderRuntime.CreateButton(modeRow.transform, "ClearBtn", "CLEAR", X13Theme.ButtonSecondaryBg, X13Theme.TextPrimary, null, 72f, 22);

            var assessBtn = X13UIBuilderRuntime.CreateButton(
                body,
                "AssessSafetyBtn",
                "SELECT BOTH REAGENTS TO ASSESS",
                X13Theme.ButtonDisabledBg,
                X13Theme.TextSecondary,
                () =>
                {
                    if (sel.HasBothSelections)
                    {
                        navigate(X13SceneIds.SafetyAssessment);
                    }
                },
                96f,
                28);

            void RefreshUI()
            {
                var chemA = sel.GetChemicalA();
                var chemB = sel.GetChemicalB();
                slotAText.text = chemA != null ? $"CHEMICAL A: {chemA.chemicalName} ({chemA.formula})" : "CHEMICAL A: Not selected";
                slotBText.text = chemB != null ? $"CHEMICAL B: {chemB.chemicalName} ({chemB.formula})" : "CHEMICAL B: Not selected";

                targetABtn.GetComponentInChildren<Text>().text = selectingSlotA ? "ASSIGNING: SLOT A" : "ASSIGN SLOT A";
                targetBBtn.GetComponentInChildren<Text>().text = !selectingSlotA ? "ASSIGNING: SLOT B" : "ASSIGN SLOT B";

                if (sel.HasBothSelections && db != null)
                {
                    var matches = db.FindExperimentsForPair(sel.ChemicalAId, sel.ChemicalBId, sel.IncludeSyntheticTestRecords);
                    if (matches.Count == 1)
                    {
                        var exp = matches[0];
                        scenarioPreview.text = $"Matched Scenario: {exp.scenarioId} ({exp.fixedQuantityA} + {exp.fixedQuantityB})\n{exp.fixedConditions}";
                    }
                    else if (matches.Count > 1)
                    {
                        scenarioPreview.text = "Ambiguous scenario records found — Safety Assessment will fail closed.";
                    }
                    else
                    {
                        scenarioPreview.text = "No predefined scenario record for this pair. Safety Assessment will mark UNKNOWN and block.";
                    }

                    assessBtn.interactable = true;
                    assessBtn.GetComponent<Image>().color = X13Theme.ButtonPrimaryBg;
                    assessBtn.GetComponentInChildren<Text>().color = X13Theme.TextDark;
                    assessBtn.GetComponentInChildren<Text>().text = "START SAFETY ASSESSMENT  ->";
                }
                else
                {
                    scenarioPreview.text = "Deliberately select both Chemical A and Chemical B. No arbitrary concentration or quantity input is permitted.";
                    assessBtn.interactable = false;
                    assessBtn.GetComponent<Image>().color = X13Theme.ButtonDisabledBg;
                    assessBtn.GetComponentInChildren<Text>().color = X13Theme.TextSecondary;
                    assessBtn.GetComponentInChildren<Text>().text = "SELECT BOTH REAGENTS TO ASSESS";
                }
            }

            targetABtn.onClick.AddListener(() => { selectingSlotA = true; RefreshUI(); });
            targetBBtn.onClick.AddListener(() => { selectingSlotA = false; RefreshUI(); });
            clearBtn.onClick.AddListener(() => { sel.ClearSelection(); selectingSlotA = true; RefreshUI(); });

            if (db != null)
            {
                var catalogue = db.GetCatalogue(sel.IncludeSyntheticTestRecords);
                var listCard = X13UIBuilderRuntime.CreateCard(body, "SelectionList", X13Theme.CardSurface, X13Theme.BorderSubtle);
                X13UIBuilderRuntime.CreateText(listCard, "Heading", $"AVAILABLE REAGENTS ({catalogue.Count})", 24, X13Theme.CyanAccent, FontStyle.Bold);

                foreach (var chem in catalogue)
                {
                    var captured = chem;
                    X13UIBuilderRuntime.CreateButton(
                        listCard,
                        $"Pick_{captured.chemicalId}",
                        $"{captured.formula}  —  {captured.chemicalName}",
                        X13Theme.CardElevated,
                        X13Theme.TextPrimary,
                        () =>
                        {
                            if (selectingSlotA)
                            {
                                sel.SelectChemicalA(captured.chemicalId);
                                selectingSlotA = false;
                            }
                            else
                            {
                                sel.SelectChemicalB(captured.chemicalId);
                            }
                            RefreshUI();
                        },
                        76f,
                        23);
                }
            }

            RefreshUI();
            X13UIBuilderRuntime.CreateBottomNav(root, 1, navigate);
        }
    }
}
