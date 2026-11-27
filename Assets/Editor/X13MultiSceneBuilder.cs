using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public static class X13MultiSceneBuilder
{
    static readonly string[] Names={"01_Splash","02_Home","03_Chemical_Library","04_Chemical_Selection","05_Safety_Assessment","06_Safety_Advisor","07_AR_Laboratory","08_Experiment_Results"};
    [MenuItem("SECTOR X-13/Generate Connected Scenes")]
    public static void Generate()
    {
        if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("SECTOR X-13","Exit Play Mode first.","OK");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        const string folder="Assets/Scenes/X13_Modules";
        Directory.CreateDirectory(folder);
        // Prevent overwriting any previously generated scenes.
        foreach(string name in Names){if(File.Exists(folder+"/X13_"+name+".unity")){EditorUtility.DisplayDialog("SECTOR X-13","Existing module scenes detected. No files changed. Move or rename the X13_Modules folder if you want a fresh generation.","OK");return;}}
        var paths=new List<string>();
        for(int i=0;i<Names.Length;i++)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("SECTOR X-13 — "+Names[i]);
            root.AddComponent<X13MultiScene>().page=i;
            string path=folder+"/X13_"+Names[i]+".unity";
            EditorSceneManager.SaveScene(scene,path);paths.Add(path);
        }
        var entries=new List<EditorBuildSettingsScene>();
        foreach(var path in paths)entries.Add(new EditorBuildSettingsScene(path,true));
        foreach(var old in EditorBuildSettings.scenes){bool found=false;foreach(var path in paths)if(old.path==path)found=true;if(!found)entries.Add(old);}
        EditorBuildSettings.scenes=entries.ToArray();
        EditorSceneManager.OpenScene(paths[0]);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("SECTOR X-13","Created 8 linked scenes under Assets/Scenes/X13_Modules. Original scenes preserved. Press Play to begin at Splash.","OK");
    }
}
