using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class X13HybridLabBuilder
{
    const string Path = "Assets/Scenes/X13_Modules/X13_07_AR_Laboratory.unity";
    [MenuItem("SECTOR X-13/Upgrade Hybrid Laboratory (Safe)")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Path);
        if (asset == null) { EditorUtility.DisplayDialog("SECTOR X-13", "Cannot find " + Path, "OK"); return; }
        var scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Single);
        var old = GameObject.Find("X13 Hybrid Lab Visuals");
        if (old != null && !EditorUtility.DisplayDialog("SECTOR X-13", "Existing hybrid visuals found. Rebuild them?", "Rebuild", "Cancel")) return;
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("X13 Hybrid Lab Visuals");
        var lab = root.AddComponent<X13HybridLab>();
        var camObj = new GameObject("Hybrid Lab Camera"); camObj.transform.SetParent(root.transform);
        var cam = camObj.AddComponent<Camera>(); cam.tag = "MainCamera";
        cam.fieldOfView = 44f; cam.nearClipPlane = .03f; cam.farClipPlane = 100f;
        camObj.transform.position = new Vector3(0f,1.85f,-4.6f);
        camObj.transform.LookAt(new Vector3(0f,.65f,0f));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.015f,.035f,.075f);
        lab.labCamera = cam;
        var light = new GameObject("Soft Key Light"); light.transform.SetParent(root.transform);
        var l = light.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.6f;
        light.transform.rotation = Quaternion.Euler(45f,-30f,0f);
        Make(PrimitiveType.Cube,"Laboratory Bench",root.transform,new Vector3(0,-.13f,0),new Vector3(3.4f,.22f,2f),new Color(.065f,.12f,.19f));
        Make(PrimitiveType.Cube,"Rear Display Panel",root.transform,new Vector3(0,1.35f,.98f),new Vector3(3.4f,1.8f,.07f),new Color(.025f,.07f,.12f));
        var left = new GameObject("Beaker A - HCl / selected acid"); left.transform.SetParent(root.transform); left.transform.position = new Vector3(-.75f,0f,0f);
        var right = new GameObject("Beaker B - NaOH / selected reagent"); right.transform.SetParent(root.transform); right.transform.position = new Vector3(.75f,0f,0f);
        lab.leftBeaker = left.transform; lab.rightBeaker = right.transform;
        MakeBeaker(left.transform,new Color(.07f,.72f,.98f),out lab.leftLiquid);
        MakeBeaker(right.transform,new Color(.19f,.94f,.65f),out lab.rightLiquid);
        lab.pourStream = Make(PrimitiveType.Cylinder,"Virtual Transfer Stream",root.transform,new Vector3(0,.7f,0),new Vector3(.035f,.3f,.035f),new Color(.15f,.88f,.95f)).transform;
        lab.reactionGlow = Make(PrimitiveType.Cylinder,"Reaction Indicator",right.transform,new Vector3(0,.54f,0),new Vector3(.35f,.015f,.35f),new Color(.17f,1f,.73f)).transform;
        lab.pourStream.gameObject.SetActive(false); lab.reactionGlow.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        EditorUtility.DisplayDialog("SECTOR X-13", "Hybrid lab created in the existing scene. Press Play to test. Other scenes and scripts were not changed.", "OK");
    }
    static GameObject Make(PrimitiveType kind,string name,Transform parent,Vector3 pos,Vector3 scale,Color color,bool transparent=false)
    {
        var obj = GameObject.CreatePrimitive(kind); obj.name=name; obj.transform.SetParent(parent,false); obj.transform.localPosition=pos; obj.transform.localScale=scale;
        var shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard");
        if(shader!=null)
        {
            var mat=new Material(shader); mat.name="X13 " + name;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",color);
            else mat.color=color;
            if(transparent)
            {
                if(mat.HasProperty("_Surface")) {mat.SetFloat("_Surface",1f);mat.SetFloat("_Blend",0f);mat.SetFloat("_ZWrite",0f);mat.renderQueue=3000;mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");}
                else {mat.SetFloat("_Mode",3f);mat.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);mat.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);mat.SetInt("_ZWrite",0);mat.renderQueue=3000;}
            }
            obj.GetComponent<Renderer>().sharedMaterial=mat;
        }
        return obj;
    }
    static void MakeBeaker(Transform parent,Color liquidColor,out Transform liquid)
    {
        var glass = new Color(.52f,.9f,1f,.22f);
        Make(PrimitiveType.Cylinder,"Glass Body",parent,new Vector3(0,.36f,0),new Vector3(.45f,.36f,.45f),glass,true);
        // A thin, visible rim and a foot make the schematic glassware legible from a distance.
        Make(PrimitiveType.Cylinder,"Top Rim",parent,new Vector3(0,.73f,0),new Vector3(.49f,.018f,.49f),new Color(.4f,.9f,1f,.8f),true);
        Make(PrimitiveType.Cylinder,"Glass Base",parent,new Vector3(0,.025f,0),new Vector3(.45f,.025f,.45f),new Color(.35f,.72f,.85f,.65f),true);
        liquid=Make(PrimitiveType.Cylinder,"Chemical Liquid",parent,new Vector3(0,.26f,0),new Vector3(.31f,.24f,.31f),liquidColor).transform;
    }
}
