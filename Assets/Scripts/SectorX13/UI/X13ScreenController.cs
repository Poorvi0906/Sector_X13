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
            var body = X13UIBuilderRuntime.CreateScrollBody(root, 120f, 120f);

            var hero = X13UIBuilderRuntime.CreateCard(body, "SplashCard", X13Theme.CardSurface, X13Theme.CyanAccent, 680f);
            X13UIBuilderRuntime.CreateText(hero, "Tag", "SAFE VIRTUAL CHEMISTRY", 26, X13Theme.CyanAccent, FontStyle.Bold, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(hero, "Title", "SECTOR X-13", 76, X13Theme.TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(hero, "Subtitle", "AR-Based Safe Chemistry Experimentation System", 30, X13Theme.TextSecondary, FontStyle.Normal, TextAnchor.MiddleCenter);
            X13UIBuilderRuntime.CreateText(hero, "Disclaimer", "Educational virtual laboratory only. Evaluates predefined scenarios before permitting virtual mixing.", 24, X13Theme.TextSecondary, FontStyle.Italic, TextAnchor.MiddleCenter);

            X13UIBuilderRuntime.CreateButton(body, "EnterLabBtn", "ENTER LABORATORY  ->", X13Theme.ButtonPrimaryBg, X13Theme.TextDark, () => Navigate(X13SceneIds.Home), 110f, 34);
        }

        private void BuildHome(RectTransform root)
        {
            X13UIBuilderRuntime.CreateHeader(root, "SECTOR X-13", "AR CHEMISTRY SYSTEM", ModeBadge, showBack: false, onBack: null);
            var body = X13UIBuilderRuntime.CreateScrollBody(root, X13Theme.HeaderHeight, X13Theme.BottomNavHeight);

            var greet = X13UIBuilderRuntime.CreateCard(body, "GreetingCard", X13Theme.BackgroundNavy, X13Theme.BackgroundNavy);
            X13UIBuilderRuntime.CreateText(greet, "Welcome", "RESEARCHER WORKSPACE", 24, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(greet, "Headline", "Ready to explore\n<color=#05D9F5>the unseen?</color>", 52, X13Theme.TextPrimary, FontStyle.Bold);

            var startCard = X13UIBuilderRuntime.CreateCard(body, "StartExperimentCard", X13Theme.CardSurface, X13Theme.CyanAccent);
            X13UIBuilderRuntime.CreateText(startCard, "Tag", "NEW SESSION", 22, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(startCard, "Desc", "Select two reagents for predefined scenario safety assessment.", 26, X13Theme.TextSecondary);
            X13UIBuilderRuntime.CreateButton(startCard, "StartExperimentBtn", "START EXPERIMENT  ->", X13Theme.ButtonPrimaryBg, X13Theme.TextDark, () => Navigate(X13SceneIds.ChemicalSelection), 96f, 32);

            var libCard = X13UIBuilderRuntime.CreateCard(body, "LibraryCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(libCard, "Tag", "CHEMICAL LIBRARY", 22, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(libCard, "Desc", "Browse reviewed reference profiles, hazards, and SDS summaries.", 26, X13Theme.TextSecondary);
            X13UIBuilderRuntime.CreateButton(libCard, "OpenLibraryBtn", "OPEN CHEMICAL LIBRARY", X13Theme.ButtonSecondaryBg, X13Theme.CyanAccent, () => Navigate(X13SceneIds.ChemicalLibrary), 88f, 28);

            // Per specification: do NOT imply a completed or verified experiment when none exists.
            var recentCard = X13UIBuilderRuntime.CreateCard(body, "RecentExperimentCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(recentCard, "Tag", "RECENT EXPERIMENTS", 22, X13Theme.TextSecondary, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(recentCard, "EmptyState", "No completed experiment in this session.", 28, X13Theme.TextPrimary, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(recentCard, "EmptyHint", "All mixing is fail-closed until a reviewed scenario is assessed and authorized.", 24, X13Theme.TextSecondary);

            var safetyCard = X13UIBuilderRuntime.CreateCard(body, "SafetyNoticeCard", X13Theme.CardSurface, X13Theme.BorderSubtle);
            X13UIBuilderRuntime.CreateText(safetyCard, "Tag", "SAFETY PROTOCOL ACTIVE", 22, X13Theme.CyanAccent, FontStyle.Bold);
            X13UIBuilderRuntime.CreateText(safetyCard, "Desc", "Simulations are gated by exact scenario IDs and scientific review status. Not a chemical prediction engine.", 24, X13Theme.TextSecondary);

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
