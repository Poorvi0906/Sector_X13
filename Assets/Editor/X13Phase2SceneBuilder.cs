using System.Collections.Generic;
using SectorX13.Core;
using SectorX13.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SectorX13.Editor
{
    /// <summary>
    /// Configures the 8 canonical X13_Modules scenes with X13ScreenController,
    /// removes legacy X13MultiScene components, and sets EditorBuildSettings to ONLY those 8 scenes.
    /// </summary>
    public static class X13Phase2SceneBuilder
    {
        [MenuItem("SECTOR X-13/Phase 2/Bake Canonical 8 Scenes")]
        public static void BuildAllCanonicalScenes()
        {
            var buildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < X13SceneIds.OrderedScenes.Length; i++)
            {
                string sceneName = X13SceneIds.OrderedScenes[i];
                string scenePath = X13SceneIds.GetScenePath(sceneName);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Remove any legacy X13MultiScene / SectorX13Simulation components to eliminate conflicting paths.
                foreach (var legacy in Object.FindObjectsByType<X13MultiScene>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    Object.DestroyImmediate(legacy);
                }
                foreach (var legacySim in Object.FindObjectsByType<SectorX13Simulation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    Object.DestroyImmediate(legacySim);
                }

                // Ensure single X13ScreenController exists with the canonical pageIndex.
                var controllers = Object.FindObjectsByType<X13ScreenController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                X13ScreenController controller = null;
                if (controllers.Length > 0)
                {
                    controller = controllers[0];
                    for (int c = 1; c < controllers.Length; c++)
                    {
                        Object.DestroyImmediate(controllers[c].gameObject);
                    }
                }
                else
                {
                    var rootGo = new GameObject($"SECTOR_X13_{sceneName}");
                    controller = rootGo.AddComponent<X13ScreenController>();
                }
                controller.pageIndex = i;

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"[X13 Phase 2] Baked canonical scene [{i}] {scenePath}");
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[X13 Phase 2] Updated EditorBuildSettings with 8 canonical scenes.");
        }
    }
}
