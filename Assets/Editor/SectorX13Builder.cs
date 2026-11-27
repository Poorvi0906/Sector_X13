using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class SectorX13Builder
{
    [MenuItem("SECTOR X-13/Generate Laboratory Simulation")]
    public static void Generate()
    {
        if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("SECTOR X-13","Exit Play mode first.","OK");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var go=new GameObject("SECTOR X-13 Application");
        go.AddComponent<SectorX13Simulation>();
        Directory.CreateDirectory("Assets/Scenes");
        var path="Assets/Scenes/X13_Laboratory_Simulation.unity";
        EditorSceneManager.SaveScene(scene,path);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("SECTOR X-13","Created "+path+". Press Play to launch. Your original AR_Laboratory scene remains unchanged.","OK");
    }
}
