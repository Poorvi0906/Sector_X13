using System.Collections;
using SectorX13.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SectorX13.UI
{
    /// <summary>
    /// Canonical per-scene UI controller for the 8 SECTOR X-13 screens.
    /// Replaces legacy OnGUI prototypes with responsive portrait uGUI.
    /// </summary>
    public class X13ScreenController : MonoBehaviour
    {
        [Range(0, 7)]
        public int pageIndex;

        [SerializeField]
        private float splashAutoAdvanceSeconds = 2.0f;

        private Canvas _canvas;
        private bool _splashNavigated;

        private static string ModeBadge => Application.isEditor ? "EDITOR SIM" : "ONLINE";

        private void Awake()
        {
            // Ensure persistent managers exist even when launching any scene directly in Editor.
            _ = SceneNavigationController.Instance;
            _ = ChemicalSelectionManager.Instance;
            _ = ExperimentManager.Instance;
            EnsureMainCamera();
        }

        private void Start()
        {
            BuildUI();
            if (pageIndex == 0 && splashAutoAdvanceSeconds > 0f)
            {
                StartCoroutine(AutoAdvanceSplash());
            }
        }

        private IEnumerator AutoAdvanceSplash()
        {
            yield return new WaitForSeconds(splashAutoAdvanceSeconds);
            if (!_splashNavigated)
            {
                _splashNavigated = true;
                SceneNavigationController.Instance.NavigateTo(X13SceneIds.Home, recordHistory: false);
            }
        }

        private void EnsureMainCamera()
        {
            if (pageIndex == 6) return; // 07_AR_Laboratory manages its own 3D/AR camera.

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = X13Theme.BackgroundNavy;
        }

        private void BuildUI()
        {
            _canvas = X13UIBuilderRuntime.CreatePortraitCanvas($"X13_Canvas_{pageIndex:00}");

            // For 07_AR_Laboratory, keep background transparent so the 3D/AR viewport is visible.
            Color rootBg = pageIndex == 6 ? new Color(0f, 0f, 0f, 0f) : X13Theme.BackgroundNavy;
            var root = X13UIBuilderRuntime.CreatePanel(_canvas.transform, "RootPanel", rootBg);

            switch (pageIndex)
            {
                case 0: BuildSplash(root); break;
                case 1: BuildHome(root); break;
                case 2: BuildChemicalLibrary(root); break;
                case 3: BuildChemicalSelection(root); break;
                case 4: BuildSafetyAssessment(root); break;
                case 5: BuildSafetyAdvisor(root); break;
                case 6: BuildARLaboratoryHUD(root); break;
                case 7: BuildExperimentResults(root); break;
            }
        }

        private void Navigate(string sceneName)
        {
            _splashNavigated = true;
            SceneNavigationController.Instance.NavigateTo(sceneName);
        }

        private void GoBack()
        {
            SceneNavigationController.Instance.GoBack();
        }

        private void BuildSplash(RectTransform root)
        {
            var body = X13UIBuilderRuntime.CreateScrollBody(root, 60f, 60f);

            X13UIBuilderRuntime.CreateText(body, "TopTag", "AR CHEMISTRY SYSTEM · v2.4", 24, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);

            var iconWrap = X13UIBuilderRuntime.CreateRow(body, "AtomRow");
            X13UIBuilderRuntime.CreateAtomIcon(iconWrap, "SplashAtom", 260f);

            X13UIBuilderRuntime.CreateText(body, "PlatformTag", "EDUCATIONAL SIMULATION PLATFORM", 22, X13Theme.TextSecondary, FontStyle.Bold, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(body, "Title", "SECTOR X-13", 76, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(body, "Subtitle", "Explore Chemistry. Understand Risk.", 30, X13Theme.TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);

            var calRow = X13UIBuilderRuntime.CreateRow(body, "CalRow");
            X13UIBuilderRuntime.CreateText(calRow, "CalLabel", "SYSTEMS CALIBRATED", 21, X13Theme.TextSecondary, FontStyle.Bold, TextAnchor.MiddleLeft);
            X13UIBuilderRuntime.CreateText(calRow, "CalPct", "100%", 21, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleRight);

            var barBg = X13UIBuilderRuntime.CreateCard(body, "ProgressBar", X13Theme.CyanAccent, X13Theme.CyanAccent, 12f, 6);
            barBg.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);

            X13UIBuilderRuntime.CreateButton(body, "EnterLabBtn", "ENTER LAB  ->", X13Theme.ButtonPrimaryBg, X13Theme.TextDark, () => Navigate(X13SceneIds.Home), 104f, 32, primaryGradient: true);
            X13UIBuilderRuntime.CreateText(body, "Footer", "VIRTUAL ENVIRONMENT · SUPERVISED LEARNING", 20, X13Theme.TextSecondary, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private void BuildHome(RectTransform root)
        {
            X13UIBuilderRuntime.CreateHeader(root, "SECTOR X-13", "AR CHEMISTRY SYSTEM", ModeBadge, showBack: false, onBack: null);
            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            var heroRow = X13UIBuilderRuntime.CreateRow(body, "HeroRow", 16f);
            var heroLeft = new GameObject("HeroLeft", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            heroLeft.transform.SetParent(heroRow, false);
            heroLeft.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var hlVlg = heroLeft.GetComponent<VerticalLayoutGroup>();
            hlVlg.spacing = 8f;
            hlVlg.childControlWidth = true;
            hlVlg.childControlHeight = true;
            X13UIBuilderRuntime.CreateText(heroLeft.transform, "Welcome", "GOOD MORNING, RESEARCHER", 22, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(heroLeft.transform, "Headline", "Ready to explore\n<color=#05D9F5>the unseen?</color>", 50, X13Theme.TextPrimary, FontStyle.Bold);
            X13UIBuilderRuntime.CreateAtomIcon(heroRow, "HomeAtom", 148f);

            var startCard = X13UIBuilderRuntime.CreateCard(body, "StartExperimentCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(startCard, "Tag", "NEW SESSION", 21, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateButton(startCard, "StartExperimentBtn", "START EXPERIMENT  ->", X13Theme.CardSurface, X13Theme.CyanAccent, () => Navigate(X13SceneIds.ChemicalSelection), 72f, 34);
            X13UIBuilderRuntime.CreateText(startCard, "Desc", "Configure a virtual reaction", 24, X13Theme.TextSecondary);

            var libCard = X13UIBuilderRuntime.CreateCard(body, "LibraryCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateButton(libCard, "OpenLibraryBtn", "CHEMICAL LIBRARY\n<size=22><color=#8FA8B8>10 reference profiles</color></size>", X13Theme.CardSurface, X13Theme.CyanAccent, () => Navigate(X13SceneIds.ChemicalLibrary), 108f, 26);

            // Per specification: never fabricate a completed/verified experiment when none exists.
            var recentHeader = X13UIBuilderRuntime.CreateRow(body, "RecentHeader");
            X13UIBuilderRuntime.CreateText(recentHeader, "Title", "RECENT EXPERIMENTS", 24, X13Theme.TextPrimary, FontStyle.Bold);

            var recentCard = X13UIBuilderRuntime.CreateCard(body, "RecentExperimentCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            if (ExperimentManager.Instance.HasCompletedScenario)
            {
                var exp = ExperimentManager.Instance.CurrentAssessment.BoundExperiment;
                X13UIBuilderRuntime.CreateText(recentCard, "Tag", $"{exp.scenarioId} · VIRTUAL", 21, X13Theme.CyanAccent, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(recentCard, "Title", exp.title, 28, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreatePillBadge(recentCard, "RiskPill", exp.reviewedRiskCategory.ToString().ToUpperInvariant(), SafetyAssessmentView.GetRiskColor(exp.reviewedRiskCategory));
            }
            else
            {
                X13UIBuilderRuntime.CreateText(recentCard, "EmptyState", "No completed experiment in this session.", 26, X13Theme.TextPrimary, FontStyle.Bold);
                X13UIBuilderRuntime.CreateText(recentCard, "EmptyHint", "All simulations fail closed until a reviewed scenario is assessed.", 22, X13Theme.TextSecondary);
            }

            var safetyCard = X13UIBuilderRuntime.CreateCard(body, "SafetyNoticeCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(safetyCard, "Tag", "SAFETY PROTOCOL ACTIVE", 21, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(safetyCard, "Desc", "All simulations are checked against experiment-specific presets.", 23, X13Theme.TextSecondary);

            X13UIBuilderRuntime.CreateBottomNav(root, 0, Navigate);
        }

        private void BuildChemicalLibrary(RectTransform root)
        {
            ChemicalLibraryView.Build(root, Navigate, GoBack);
        }

        private void BuildChemicalSelection(RectTransform root)
        {
            ChemicalSelectionView.Build(root, Navigate, GoBack);
        }

        private void BuildSafetyAssessment(RectTransform root)
        {
            SafetyAssessmentView.Build(root, Navigate, GoBack);
        }

        private void BuildSafetyAdvisor(RectTransform root)
        {
            SafetyAdvisorView.Build(root, Navigate, GoBack);
        }

        private void BuildARLaboratoryHUD(RectTransform root)
        {
            ARLabHUDView.Build(root, Navigate, GoBack);
        }

        private void BuildExperimentResults(RectTransform root)
        {
            ExperimentResultsView.Build(root, Navigate, GoBack);
        }
    }
}
