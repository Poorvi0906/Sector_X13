using SectorX13.AR;
using SectorX13.Core;
using SectorX13.Lab;
using SectorX13.UI;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SectorX13.Editor
{
    /// <summary>
    /// Rebuilds X13_07_AR_Laboratory.unity with 3 hollow glass beakers (Beaker A, Reaction Chamber, Beaker B),
    /// a glowing holographic pad, URP particle bubbles, and zero speckle noise.
    /// </summary>
    public static class X13Phase6LabBuilder
    {
        [MenuItem("SECTOR X-13/Phase 6/Rebuild 07_AR_Laboratory Scene")]
        public static void RebuildARLabScene()
        {
            const string genDir = "Assets/Generated/Lab3D";
            System.IO.Directory.CreateDirectory(genDir);

            string scenePath = X13SceneIds.GetScenePath(X13SceneIds.ARLaboratory);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var uiRoot = new GameObject($"SECTOR_X13_{X13SceneIds.ARLaboratory}");
            uiRoot.AddComponent<X13ScreenController>().pageIndex = 6;

            // Soft directional + fill lights (shadows off on directional to prevent mobile URP speckle)
            var lightGo = new GameObject("Lab Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.None;
            light.color = new Color(0.95f, 0.98f, 1.0f);
            lightGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            var arSessionGo = new GameObject("AR Session");
            arSessionGo.AddComponent<ARSession>();
            arSessionGo.AddComponent<ARInputManager>();

            var xrOriginGo = new GameObject("XR Origin");
            var xrOrigin = xrOriginGo.AddComponent<XROrigin>();
            var planeMgr = xrOriginGo.AddComponent<ARPlaneManager>();
            var raycastMgr = xrOriginGo.AddComponent<ARRaycastManager>();

            var camOffset = new GameObject("Camera Offset");
            camOffset.transform.SetParent(xrOriginGo.transform, false);
            xrOrigin.CameraFloorOffsetObject = camOffset;

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(camOffset.transform, false);
            camGo.transform.position = new Vector3(0f, 1.35f, -3.25f);
            camGo.transform.LookAt(new Vector3(0f, 0.35f, 0f));
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = X13Theme.BackgroundNavy;
            cam.nearClipPlane = 0.1f;
            cam.allowHDR = false;
            cam.allowMSAA = true;
            camGo.AddComponent<ARCameraManager>();
            camGo.AddComponent<ARCameraBackground>();
            xrOrigin.Camera = cam;

            var rigRoot = new GameObject("X13_LabRig");

            // Glowing holographic grid pad under the Reaction Chamber
            var holoPad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            holoPad.name = "HoloGridPad";
            holoPad.transform.SetParent(rigRoot.transform, false);
            holoPad.transform.localPosition = new Vector3(0f, 0f, 0f);
            holoPad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            holoPad.transform.localScale = new Vector3(2.4f, 1.5f, 1f);
            var holoMat = SaveAsset(BeakerMeshGenerator.CreateHoloPadMaterial(), "Assets/Generated/Lab3D/X13_HoloPadMat.mat");
            if (holoMat.mainTexture != null) SaveAsset(holoMat.mainTexture, "Assets/Generated/Lab3D/X13_HoloGridTex.asset");
            holoPad.GetComponent<Renderer>().sharedMaterial = holoMat;

            // Shared hollow glass beaker mesh & materials saved to disk
            Mesh smallBeakerMesh = SaveAsset(BeakerMeshGenerator.CreateHollowBeakerMesh(0.24f, 0.016f, 0.56f, 36), "Assets/Generated/Lab3D/X13_SmallBeakerMesh.asset");
            Mesh centerBeakerMesh = SaveAsset(BeakerMeshGenerator.CreateHollowBeakerMesh(0.36f, 0.020f, 0.72f, 40), "Assets/Generated/Lab3D/X13_CenterBeakerMesh.asset");
            Material glassMat = SaveAsset(BeakerMeshGenerator.CreateGlassMaterial(), "Assets/Generated/Lab3D/X13_GlassMat.mat");

            var beakerA = CreateBeakerVessel("BeakerA_Vessel", rigRoot.transform, new Vector3(-0.78f, 0f, 0f), smallBeakerMesh, glassMat);
            var liquidA = CreateLiquidCylinder("BeakerA_Liquid", beakerA.transform, new Vector3(0f, 0.16f, 0f), new Vector3(0.41f, 0.13f, 0.41f), new Color(0.10f, 0.82f, 0.95f));

            var beakerB = CreateBeakerVessel("BeakerB_Vessel", rigRoot.transform, new Vector3(0.78f, 0f, 0f), smallBeakerMesh, glassMat);
            var liquidB = CreateLiquidCylinder("BeakerB_Liquid", beakerB.transform, new Vector3(0f, 0.16f, 0f), new Vector3(0.41f, 0.13f, 0.41f), new Color(0.12f, 0.85f, 0.65f));

            var chamber = CreateBeakerVessel("ReactionChamber", rigRoot.transform, new Vector3(0f, 0f, 0f), centerBeakerMesh, glassMat);
            var liquidMix = CreateLiquidCylinder("ReactionChamber_Liquid", chamber.transform, new Vector3(0f, 0.20f, 0f), new Vector3(0.64f, 0.18f, 0.64f), new Color(0.12f, 0.88f, 0.78f));

            var streamA = CreateLiquidCylinder("StreamA", rigRoot.transform, new Vector3(-0.18f, 0.68f, 0f), new Vector3(0.035f, 0.34f, 0.035f), new Color(0.10f, 0.85f, 0.98f));
            streamA.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            streamA.SetActive(false);

            var streamB = CreateLiquidCylinder("StreamB", rigRoot.transform, new Vector3(0.18f, 0.68f, 0f), new Vector3(0.035f, 0.34f, 0.035f), new Color(0.45f, 0.35f, 0.98f));
            streamB.transform.localRotation = Quaternion.Euler(0f, 0f, -22f);
            streamB.SetActive(false);

            var glow = CreateLiquidCylinder("ReactionGlow", chamber.transform, new Vector3(0f, 0.02f, 0f), new Vector3(0.76f, 0.01f, 0.76f), new Color(0.05f, 0.95f, 0.75f));
            glow.SetActive(false);

            // Soft circular URP bubble particle system (zero magenta squares!)
            var particlesGo = new GameObject("BubbleParticles");
            particlesGo.transform.SetParent(chamber.transform, false);
            particlesGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            var ps = particlesGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            main.startLifetime = 1.1f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.24f;
            var psRenderer = particlesGo.GetComponent<ParticleSystemRenderer>();
            var bubbleMat = SaveAsset(BeakerMeshGenerator.CreateBubbleParticleMaterial(), "Assets/Generated/Lab3D/X13_BubbleMat.mat");
            if (bubbleMat.mainTexture != null) SaveAsset(bubbleMat.mainTexture, "Assets/Generated/Lab3D/X13_BubbleTex.asset");
            psRenderer.sharedMaterial = bubbleMat;

            var reactionCtrl = rigRoot.AddComponent<ReactionController>();
            reactionCtrl.beakerA = beakerA.transform;
            reactionCtrl.beakerB = beakerB.transform;
            reactionCtrl.reactionChamber = chamber.transform;
            reactionCtrl.liquidA = liquidA.transform;
            reactionCtrl.liquidB = liquidB.transform;
            reactionCtrl.liquidMix = liquidMix.transform;
            reactionCtrl.streamA = streamA.transform;
            reactionCtrl.streamB = streamB.transform;
            reactionCtrl.bubbleParticles = ps;
            reactionCtrl.liquidARenderer = liquidA.GetComponent<Renderer>();
            reactionCtrl.liquidBRenderer = liquidB.GetComponent<Renderer>();
            reactionCtrl.liquidMixRenderer = liquidMix.GetComponent<Renderer>();
            reactionCtrl.reactionGlowRenderer = glow.GetComponent<Renderer>();

            var placement = rigRoot.AddComponent<ARPlacementManager>();
            placement.labRigRoot = rigRoot.transform;
            placement.raycastManager = raycastMgr;
            placement.planeManager = planeMgr;
            placement.arCamera = cam;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[X13 Phase 11] Rebuilt X13_07_AR_Laboratory.unity with 3D hollow glass beakers.");
        }

        private static GameObject CreateBeakerVessel(string name, Transform parent, Vector3 localPos, Mesh mesh, Material glassMat)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
            return go;
        }

        private static GameObject CreateLiquidCylinder(string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = SaveAsset(BeakerMeshGenerator.CreateLiquidMaterial(color), $"Assets/Generated/Lab3D/{name}_Mat.mat");
            return go;
        }

        private static T SaveAsset<T>(T obj, string path) where T : Object
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(obj, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
