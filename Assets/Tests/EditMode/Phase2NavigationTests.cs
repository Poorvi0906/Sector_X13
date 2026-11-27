using NUnit.Framework;
using SectorX13.Core;
using SectorX13.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SectorX13.Tests.EditMode
{
    public class Phase2NavigationTests
    {
        [Test]
        public void CanonicalScenes_AreExactEightInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            Assert.AreEqual(8, scenes.Length, "Build Settings must contain exactly the 8 canonical X13_Modules scenes.");

            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(scenes[i].enabled, $"Scene index {i} must be enabled.");
                Assert.AreEqual(X13SceneIds.GetScenePath(X13SceneIds.OrderedScenes[i]), scenes[i].path);
            }
        }

        [Test]
        public void CanonicalScenes_HaveScreenControllerAndNoLegacyMultiScene()
        {
            for (int i = 0; i < X13SceneIds.OrderedScenes.Length; i++)
            {
                string path = X13SceneIds.GetScenePath(X13SceneIds.OrderedScenes[i]);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Assert.IsTrue(scene.IsValid(), $"Scene {path} failed to open.");

                var controllers = Object.FindObjectsByType<X13ScreenController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Assert.AreEqual(1, controllers.Length, $"Scene {path} must have exactly 1 X13ScreenController.");
                Assert.AreEqual(i, controllers[0].pageIndex, $"Scene {path} pageIndex mismatch.");

                var allBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var mb in allBehaviours)
                {
                    Assert.AreNotEqual("X13MultiScene", mb.GetType().Name, $"Scene {path} must not contain legacy X13MultiScene.");
                    Assert.AreNotEqual("SectorX13Simulation", mb.GetType().Name, $"Scene {path} must not contain legacy SectorX13Simulation.");
                }
            }
        }

        [Test]
        public void X13SceneIds_MapsAllEightPages()
        {
            Assert.AreEqual(0, X13SceneIds.GetPageIndex(X13SceneIds.Splash));
            Assert.AreEqual(1, X13SceneIds.GetPageIndex(X13SceneIds.Home));
            Assert.AreEqual(6, X13SceneIds.GetPageIndex(X13SceneIds.ARLaboratory));
            Assert.AreEqual(7, X13SceneIds.GetPageIndex(X13SceneIds.ExperimentResults));
            Assert.AreEqual(-1, X13SceneIds.GetPageIndex("NonExistentScene"));
        }
    }
}
