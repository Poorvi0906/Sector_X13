using System.Collections.Generic;
using SectorX13.Core;
using SectorX13.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Renders the 10-chemical catalogue and 6-section Chemical Profile for 03_Chemical_Library.
    /// </summary>
    public static class ChemicalLibraryView
    {
        private static readonly string[] SectionTitles =
        {
            "Basic Info",
            "Properties",
            "Safety Info",
            "Experiment Info",
            "AR Info",
            "Risk Assessment"
        };

        public static void Build(RectTransform root, System.Action<string> navigate, System.Action goBack)
        {
            X13UIBuilderRuntime.CreateHeader(root, "CHEMICAL LIBRARY", "10 REFERENCE PROFILES", Application.isEditor ? "EDITOR SIM" : "ONLINE", true, goBack);
            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            var db = ChemicalDatabase.Load();
            var sel = ChemicalSelectionManager.Instance;
            if (db == null)
            {
                var err = X13UIBuilderRuntime.CreateCard(body, "DbMissing", X13Theme.CardSurface, X13Theme.RiskHigh);
                X13UIBuilderRuntime.CreateText(err, "Msg", "Database asset not found at Resources/X13Data/X13Database.", 26, X13Theme.RiskHigh, FontStyle.Bold);
                X13UIBuilderRuntime.CreateBottomNav(root, 2, navigate);
                return;
            }

            var catalogue = db.GetCatalogue(includeSynthetic: false);
            var activeChem = sel.GetInspectedChemical() ?? (catalogue.Count > 0 ? catalogue[0] : null);

            // Profile card with 6 information sections
            var profileCard = X13UIBuilderRuntime.CreateCard(body, "ProfileCard", X13Theme.CardSurface, X13Theme.CyanAccent);
            var chemTitle = X13UIBuilderRuntime.CreateText(profileCard, "ChemTitle", "", 36, X13Theme.TextPrimary, FontStyle.Bold);
            var chemSub = X13UIBuilderRuntime.CreateText(profileCard, "ChemSub", "", 24, X13Theme.CyanAccent, FontStyle.Bold);
            var sectionHeader = X13UIBuilderRuntime.CreateText(profileCard, "SectionHeader", "", 22, X13Theme.CyanAccent, FontStyle.Bold);
            var sectionBody = X13UIBuilderRuntime.CreateText(profileCard, "SectionBody", "", 24, X13Theme.TextSecondary);

            int activeTab = 0;

            void RefreshProfile(ChemicalData chem, int tab)
            {
                if (chem == null) return;
                activeChem = chem;
                activeTab = tab;
                sel.InspectChemical(chem.chemicalId);

                chemTitle.text = $"{chem.chemicalName} ({chem.formula})";
                chemSub.text = $"{chem.chemicalId} · {chem.category} · Review: {chem.reviewStatus}";
                sectionHeader.text = $"[{tab + 1}/6] {SectionTitles[tab].ToUpperInvariant()}";
                sectionBody.text = GetSectionContent(chem, tab);
            }

            // 6-section tab bar (2 rows of 3)
            for (int row = 0; row < 2; row++)
            {
                var rowGo = new GameObject($"TabRow_{row}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                rowGo.transform.SetParent(profileCard, false);
                var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;

                for (int col = 0; col < 3; col++)
                {
                    int idx = row * 3 + col;
                    X13UIBuilderRuntime.CreateButton(
                        rowGo.transform,
                        $"Tab_{idx}",
                        SectionTitles[idx],
                        X13Theme.ButtonSecondaryBg,
                        X13Theme.CyanAccent,
                        () => RefreshProfile(activeChem, idx),
                        68f,
                        20);
                }
            }

            // Action row: Use in Experiment
            X13UIBuilderRuntime.CreateButton(
                profileCard,
                "SelectForExpBtn",
                "SELECT FOR EXPERIMENT  ->",
                X13Theme.ButtonPrimaryBg,
                X13Theme.TextDark,
                () =>
                {
                    if (activeChem != null) sel.SelectChemicalA(activeChem.chemicalId);
                    navigate(X13SceneIds.ChemicalSelection);
                },
                82f,
                26);

            // Catalogue list
            var listCard = X13UIBuilderRuntime.CreateCard(body, "CatalogueList", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(listCard, "ListHeading", $"CATALOGUE ({catalogue.Count} COMPOUNDS)", 24, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(listCard, "ListNote", "Catalogue membership does not imply permission to mix.", 21, X13Theme.TextSecondary);

            foreach (var chem in catalogue)
            {
                var captured = chem;
                X13UIBuilderRuntime.CreateButton(
                    listCard,
                    $"Item_{captured.chemicalId}",
                    $"{captured.formula}  —  {captured.chemicalName}  ({captured.category})",
                    X13Theme.CardElevated,
                    X13Theme.TextPrimary,
                    () => RefreshProfile(captured, activeTab),
                    78f,
                    23);
            }

            RefreshProfile(activeChem, 0);
            X13UIBuilderRuntime.CreateBottomNav(root, 2, navigate);
        }

        public static string GetSectionContent(ChemicalData c, int sectionIndex)
        {
            switch (sectionIndex)
            {
                case 0: // Basic Info
                    return $"Identifier: {c.chemicalId}\nName: {c.chemicalName}\nFormula: {c.formula}\nCategory: {c.category}";
                case 1: // Properties
                    return $"Form: {c.physicalForm}\nProperties: {c.propertiesSummary}\nSource: {c.sourceReference}";
                case 2: // Safety Info
                    return $"Hazards: {c.hazardInfo}\nPrecautions: {c.precautions}\nSDS: {c.sdsReference}\nReview Status: {c.reviewStatus}";
                case 3: // Experiment Info
                    string refs = (c.supportedScenarioIds != null && c.supportedScenarioIds.Length > 0)
                        ? string.Join(", ", c.supportedScenarioIds)
                        : "None";
                    return $"Scenario References: {refs}\nConditions: {c.fixedConditionsNote}";
                case 4: // AR Info
                    return $"Visual Representation: {c.arVisualDescription}\nVisual Asset ID: {c.visualAssetId}";
                case 5: // Risk Assessment
                    return $"Assessment: {c.riskAssessmentSummary}";
                default:
                    return string.Empty;
            }
        }
    }
}
