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
    /// Rebuilds X13_07_AR_Laboratory.unity with a clean URP 3D rig (fixing speckle artifacts),
    /// ARSession/XROrigin placement support, and ReactionController.
    /// </summary>
    public static class X13Phase6LabBuilder
    {
        [MenuItem("SECTOR X-13/Phase 6/Rebuild 07_AR_Laboratory Scene")]
        public static void RebuildARLabScene()
        {
            string scenePath = X13SceneIds.GetScenePath(X13SceneIds.ARLaboratory);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Screen UI controller
            var uiRoot = new GameObject($"SECTOR_X13_{X13SceneIds.ARLaboratory}");
            uiRoot.AddComponent<X13ScreenController>().pageIndex = 6;

            // Lighting
            var lightGo = new GameObject("Lab Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(0.95f, 0.98f, 1.0f);
            lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // ARSession + XROrigin (Android AR path + Editor camera)
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
            camGo.transform.position = new Vector3(0f, 1.35f, -3.2f);
            camGo.transform.LookAt(new Vector3(0f, 0.25f, 0f));
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = X13Theme.BackgroundNavy;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<ARCameraManager>();
            camGo.AddComponent<ARCameraBackground>();
            xrOrigin.Camera = cam;

            // 3D Lab Rig
            var rigRoot = new GameObject("X13_LabRig");
            var bench = CreatePrimitive(PrimitiveType.Cube, "LabBench", rigRoot.transform, new Vector3(0f, -0.1f, 0f), new Vector3(2.4f, 0.16f, 1.2f), new Color(0.08f, 0.16f, 0.24f));
            var beakerA = CreatePrimitive(PrimitiveType.Cylinder, "BeakerA_Vessel", rigRoot.transform, new Vector3(-0.55f, 0.32f, 0f), new Vector3(0.42f, 0.34f, 0.42f), new Color(0.18f, 0.30f, 0.40f));
            var liquidA = CreatePrimitive(PrimitiveType.Cylinder, "BeakerA_Liquid", beakerA.transform, new Vector3(0f, -0.15f, 0f), new Vector3(0.86f, 0.55f, 0.86f), new Color(0.10f, 0.82f, 0.95f));
            var beakerB = CreatePrimitive(PrimitiveType.Cylinder, "BeakerB_Vessel", rigRoot.transform, new Vector3(0.55f, 0.32f, 0f), new Vector3(0.42f, 0.34f, 0.42f), new Color(0.18f, 0.30f, 0.40f));
            var liquidB = CreatePrimitive(PrimitiveType.Cylinder, "BeakerB_Liquid", beakerB.transform, new Vector3(0f, -0.15f, 0f), new Vector3(0.86f, 0.55f, 0.86f), new Color(0.12f, 0.85f, 0.65f));
            var pourStream = CreatePrimitive(PrimitiveType.Cylinder, "PourStream", rigRoot.transform, new Vector3(0f, 0.58f, 0f), new Vector3(0.05f, 0.38f, 0.05f), new Color(0.10f, 0.88f, 0.98f));
            pourStream.transform.rotation = Quaternion.Euler(0f, 0f, -55f);
            pourStream.SetActive(false);

            var glow = CreatePrimitive(PrimitiveType.Sphere, "ReactionGlow", beakerB.transform, new Vector3(0f, 0.1f, 0f), new Vector3(0.65f, 0.65f, 0.65f), new Color(0.05f, 0.95f, 0.85f));
            glow.SetActive(false);

            var particlesGo = new GameObject("BubbleParticles");
            particlesGo.transform.SetParent(beakerB.transform, false);
            particlesGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            var ps = particlesGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.startSize = 0.05f;
            main.startSpeed = 0.45f;
            main.startLifetime = 1.0f;

            var reactionCtrl = rigRoot.AddComponent<ReactionController>();
            reactionCtrl.beakerA = beakerA.transform;
            reactionCtrl.beakerB = beakerB.transform;
            reactionCtrl.liquidA = liquidA.transform;
            reactionCtrl.liquidB = liquidB.transform;
            reactionCtrl.pourStream = pourStream.transform;
            reactionCtrl.bubbleParticles = ps;
            reactionCtrl.liquidARenderer = liquidA.GetComponent<Renderer>();
            reactionCtrl.liquidBRenderer = liquidB.GetComponent<Renderer>();
            reactionCtrl.reactionGlowRenderer = glow.GetComponent<Renderer>();

            var placement = rigRoot.AddComponent<ARPlacementManager>();
            placement.labRigRoot = rigRoot.transform;
            placement.raycastManager = raycastMgr;
            placement.planeManager = planeMgr;
            placement.arCamera = cam;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[X13 Phase 6] Rebuilt X13_07_AR_Laboratory.unity cleanly.");
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.35f);
                go.GetComponent<Renderer>().sharedMaterial = mat;
            }
            return go;
        }
    }
}
