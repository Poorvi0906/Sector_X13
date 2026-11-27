namespace SectorX13.Core
{
    /// <summary>
    /// Canonical scene identifiers for the 8-screen SECTOR X-13 workflow.
    /// </summary>
    public static class X13SceneIds
    {
        public const string Splash = "X13_01_Splash";
        public const string Home = "X13_02_Home";
        public const string ChemicalLibrary = "X13_03_Chemical_Library";
        public const string ChemicalSelection = "X13_04_Chemical_Selection";
        public const string SafetyAssessment = "X13_05_Safety_Assessment";
        public const string SafetyAdvisor = "X13_06_Safety_Advisor";
        public const string ARLaboratory = "X13_07_AR_Laboratory";
        public const string ExperimentResults = "X13_08_Experiment_Results";

        public static readonly string[] OrderedScenes =
        {
            Splash,
            Home,
            ChemicalLibrary,
            ChemicalSelection,
            SafetyAssessment,
            SafetyAdvisor,
            ARLaboratory,
            ExperimentResults
        };

        public static string GetScenePath(string sceneName)
        {
            return $"Assets/Scenes/X13_Modules/{sceneName}.unity";
        }

        public static int GetPageIndex(string sceneName)
        {
            for (int i = 0; i < OrderedScenes.Length; i++)
            {
                if (OrderedScenes[i] == sceneName) return i;
            }
            return -1;
        }
    }
}
