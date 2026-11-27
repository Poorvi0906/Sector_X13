using UnityEngine;
using UnityEngine.SceneManagement;

public static class X13State
{
    public static int acid = 0, reagent = 0;
    public static bool assessed = false, permitted = false, reacted = false;
    public static string risk = "UNKNOWN", note = "Not assessed", equation = "";
    public static readonly string[] Acids = { "Acetic acid (5% vinegar)", "Hydrochloric acid (0.10 M)", "Citric acid", "Sulfuric acid" };
    public static readonly string[] Reagents = { "Sodium bicarbonate", "Sodium hydroxide (0.10 M)", "Calcium hydroxide", "Magnesium hydroxide" };
    public static void Reset() { assessed = false; permitted = false; reacted = false; risk = "UNKNOWN"; note = "Not assessed"; equation = ""; }
    public static void Assess()
    {
        assessed = true; permitted = false; reacted = false;
        if (acid == 1 && reagent == 1)
        {
            risk = "MEDIUM";
            note = "Virtual-only neutralization preset: 10 mL 0.10 M HCl + 10 mL 0.10 M NaOH. Exothermic; instructor review pending.";
            equation = "HCl + NaOH -> NaCl + H2O";
        }
        else if (acid == 0 && reagent == 0)
        {
            risk = "UNKNOWN";
            note = "Vinegar + bicarbonate candidate. Exact preset and risk tier await instructor validation. CO2 generation; never use a sealed container.";
            equation = "CH3COOH + NaHCO3 -> CH3COONa + H2O + CO2";
        }
        else
        {
            risk = "UNKNOWN";
            note = "No verified concentration-specific experiment for this pair. Mixing blocked.";
            equation = "No validated reaction for these conditions.";
        }
    }
}

public class X13MultiScene : MonoBehaviour
{
    public int page;
    GUIStyle heading, body, small, btn;
    Vector2 scroll;
    float reactionTime;
    GameObject beaker1, beaker2;
    int pendingScene = -1;
    const string Prefix = "X13_";
    const float DesignWidth = 420f;
    const float DesignHeight = 820f;
    static readonly string[] Names = { "01_Splash", "02_Home", "03_Chemical_Library", "04_Chemical_Selection", "05_Safety_Assessment", "06_Safety_Advisor", "07_AR_Laboratory", "08_Experiment_Results" };

    void Go(int target) { if (target >= 0 && target < Names.Length) pendingScene = target; }

    void Start()
    {
        // A camera is required for the Game view, even when the interface uses OnGUI.
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject cameraObject = new GameObject("X13 Scene Camera");
            cameraObject.tag = "MainCamera";
            cam = cameraObject.AddComponent<Camera>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.018f, .035f, .075f);
        if (page != 6 || FindFirstObjectByType<X13HybridLab>() != null) return;

        cam.transform.position = new Vector3(0, 1.7f, -4.2f);
        cam.transform.LookAt(new Vector3(0, .3f, 0));
        Light light = new GameObject("Laboratory Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.transform.rotation = Quaternion.Euler(40, -25, 0);
        Make(PrimitiveType.Cube, "Lab Platform", new Vector3(0, -.12f, 0), new Vector3(3, .2f, 1.7f), new Color(.11f, .22f, .31f));
        beaker1 = Make(PrimitiveType.Cylinder, "Beaker A (schematic)", new Vector3(-.6f, .34f, 0), new Vector3(.42f, .33f, .42f), new Color(.07f, .57f, .7f));
        beaker2 = Make(PrimitiveType.Cylinder, "Beaker B (schematic)", new Vector3(.6f, .34f, 0), new Vector3(.42f, .33f, .42f), new Color(.12f, .75f, .57f));
    }

    GameObject Make(PrimitiveType type, string label, Vector3 pos, Vector3 scale, Color tint)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = label; obj.transform.position = pos; obj.transform.localScale = scale;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = tint;
            obj.GetComponent<Renderer>().material = mat;
        }
        return obj;
    }

    void Styles()
    {
        if (heading != null) return;
        heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        heading.normal.textColor = new Color(.05f, .9f, .98f);
        body = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        body.normal.textColor = Color.white;
        small = new GUIStyle(body) { fontSize = 12 };
        btn = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
    }
    bool B(string label) { return GUILayout.Button(label, btn, GUILayout.Height(43)); }

    Texture2D artwork;
    bool acknowledged;
    GUIStyle labelStyle, actionStyle;
    Color navy = new Color(.025f,.065f,.105f);
    Color card = new Color(.045f,.12f,.18f);
    Color cyan = new Color(.02f,.85f,.96f);
    void Fill(Rect r, Color c) { var old=GUI.color; GUI.color=c; GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color=old; }
    void Text(float x,float y,float w,float h,string text,int size,Color color) {
        labelStyle.fontSize=size; labelStyle.normal.textColor=color;
        GUI.Label(new Rect(x+6,y+2,w-12,h-4),text,labelStyle);
    }
    void Panel(float x,float y,float w,float h,string text,int size=16,bool accent=false) {
        Fill(new Rect(x,y,w,h),card); Fill(new Rect(x,y,2,h),new Color(.08f,.35f,.45f));
        Text(x,y,w,h,text,size,accent?cyan:Color.white);
    }
    bool Action(float x,float y,float w,float h,string text,bool selected=false) {
        Rect r=new Rect(x,y,w,h); Fill(r,selected?cyan:new Color(.07f,.19f,.26f));
        Text(x,y,w,h,text,15,selected?navy:cyan);
        return GUI.Button(r,GUIContent.none,GUIStyle.none);
    }
    bool Hit(float x,float y,float w,float h) { return GUI.Button(new Rect(x,y,w,h),GUIContent.none,GUIStyle.none); }
    void Nav() {
        string[] labels={"HOME","EXPERIMENT","LIBRARY","SAFETY"}; int[] targets={1,3,2,4};
        for(int i=0;i<4;i++) if(Action(i*112.5f,744,111,56,labels[i],page==targets[i])) Go(targets[i]);
    }
    void Header(string title) {
        Fill(new Rect(0,0,450,74),card);
        Text(58,8,380,48,title,22,cyan);
        if(Action(12,14,44,44,"<")) Go(page==4?3:page==5?4:1);
    }
    void OnGUI() {
        if (!gameObject.scene.IsValid() || gameObject.scene != SceneManager.GetActiveScene() || pendingScene>=0) return;
        if(labelStyle==null) labelStyle=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,wordWrap=true,fontStyle=FontStyle.Bold};
        float scale=Mathf.Min(Screen.width/450f,Screen.height/800f);
        var old=GUI.matrix; GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-450*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
        if(page!=6) Fill(new Rect(0,0,450,800),navy);
        if(page==0 || page==1) {
            if(artwork==null) artwork=Resources.Load<Texture2D>("X13Artwork/"+Names[page]);
            if(artwork!=null) GUI.DrawTexture(new Rect(0,0,450,800),artwork,ScaleMode.StretchToFill,true);
        }
        switch(page) {
            case 0:
                if(artwork==null) {Text(20,180,410,200,"SECTOR X-13",36,cyan); if(Action(35,651,380,46,"ENTER LAB",true)) Go(1);}
                else if(Hit(35,651,380,46)) Go(1); break;
            case 1:
                Fill(new Rect(0,320,450,480),navy);
                if(artwork==null) {Header("SECTOR X-13"); if(Action(18,205,414,112,"START EXPERIMENT",true)) Go(3);}
                else if(Hit(18,205,414,112)) Go(3);
                if(Action(18,333,414,76,"CHEMICAL LIBRARY")) Go(2);
                if(Action(18,430,414,112,"VIRTUAL PRESET\nDilute neutralisation - review pending")) Go(3);
                Panel(18,570,414,100,"SAFETY PROTOCOL ACTIVE\nAll experiments are virtual and preset-specific.",16); Nav(); break;
            case 2:
                Header("CHEMICAL LIBRARY");
                Panel(18,94,414,220,"ACIDS\n\nAcetic acid | Hydrochloric acid\nCitric acid | Sulfuric acid",19);
                Panel(18,330,414,240,"REAGENTS\n\nSodium bicarbonate | Sodium hydroxide\nCalcium hydroxide | Magnesium hydroxide\nReference: Potassium hydroxide",18);
                Panel(18,585,414,75,"SDS and instructor validation pending.",16);
                if(Action(18,675,414,50,"CHOOSE CHEMICALS",true)) Go(3); Nav(); break;
            case 3:
                Header("SELECT REACTANTS");
                Panel(18,90,202,100,"BEAKER A\n"+X13State.Acids[X13State.acid],16,true);
                Panel(230,90,202,100,"BEAKER B\n"+X13State.Reagents[X13State.reagent],16,true);
                Text(18,205,202,40,"ACIDS",18,cyan); Text(230,205,202,40,"REAGENTS",18,cyan);
                for(int i=0;i<4;i++) {
                    if(Action(18,255+i*90,202,78,X13State.Acids[i],X13State.acid==i)) {X13State.acid=i;X13State.Reset();}
                    if(Action(230,255+i*90,202,78,X13State.Reagents[i],X13State.reagent==i)) {X13State.reagent=i;X13State.Reset();}
                }
                if(Action(18,660,414,58,"ASSESS BEFORE MIXING",true)){X13State.Assess();Go(4);} Nav(); break;
            case 4:
                if(!X13State.assessed) X13State.Assess();
                Header("SAFETY ASSESSMENT");
                Panel(18,90,414,125,"RISK: "+X13State.risk+"\n"+(X13State.risk=="MEDIUM"?"VIRTUAL ONLY - REVIEW PENDING":"MIXING BLOCKED"),23,true);
                Panel(18,230,202,85,"A: "+X13State.Acids[X13State.acid]); Panel(230,230,202,85,"B: "+X13State.Reagents[X13State.reagent]);
                Panel(18,330,414,185,X13State.note,18);
                if(X13State.risk=="MEDIUM") {
                    Panel(18,530,414,55,"PPE: goggles, lab coat, suitable gloves",15);
                    if(Action(18,595,414,58,(acknowledged?"ACCEPTED: ":"ACKNOWLEDGE: ")+"Virtual-only precautions",acknowledged)) acknowledged=!acknowledged;
                } else Panel(18,530,414,120,"Safety lock active. Select another preset or consult the advisor.",18);
                if(Action(18,675,202,50,"SAFETY ADVISOR")) Go(5);
                if(X13State.risk=="MEDIUM" && acknowledged) {if(Action(230,675,202,50,"LAUNCH SIMULATION",true)){X13State.permitted=true;Go(6);}}
                else if(Action(230,675,202,50,"CHANGE CHEMICALS")) Go(3);
                Nav(); break;
            case 5:
                Header("SMART SAFETY ADVISOR");
                Panel(18,95,414,145,X13State.Acids[X13State.acid]+" + "+X13State.Reagents[X13State.reagent]+"\nRisk: "+X13State.risk,18);
                Panel(18,255,414,145,"Unreviewed combinations remain blocked. Dilution alone does not establish safety.",18);
                Panel(18,415,414,150,"Alternative virtual demonstration\nDilute neutralisation preset\nInstructor review pending",19,true);
                if(Action(18,580,414,60,"SELECT VIRTUAL PRESET",true)){X13State.acid=1;X13State.reagent=1;X13State.Reset();X13State.Assess();Go(4);}
                if(Action(18,660,414,58,"RETURN TO SAFETY")) Go(4); Nav(); break;
            case 6:
                bool valid=X13State.assessed && X13State.permitted && X13State.risk=="MEDIUM";
                Fill(new Rect(0,0,450,180),navy);
                Text(18,12,414,60,"AR VIRTUAL LAB - "+(valid?"MEDIUM":"LOCKED"),21,cyan);
                Panel(18,80,414,90,X13State.Acids[X13State.acid]+"\n"+X13State.Reagents[X13State.reagent],16);
                Fill(new Rect(0,610,450,190),navy);
                if(valid && !X13State.reacted) {if(Action(18,625,414,52,"START VIRTUAL REACTION",true)){X13State.reacted=true;reactionTime=Time.time;}}
                else if(valid && X13State.reacted) {if(Action(18,625,414,52,"VIEW RESULTS",true)) Go(7);}
                else Panel(18,625,414,52,"SAFETY LOCK - NO MIXING",19,true);
                if(Action(18,690,202,48,"SAFETY ASSESSMENT")) Go(4);
                if(Action(230,690,202,48,"HOME")) Go(1);
                Text(18,750,414,40,"VIRTUAL PRESET / SIMULATION ONLY",13,cyan); break;
            case 7:
                Header("EXPERIMENT RESULTS");
                bool complete=X13State.reacted && X13State.permitted && X13State.risk=="MEDIUM";
                Panel(18,95,414,125,complete?"VIRTUAL DEMONSTRATION\nRisk: "+X13State.risk:"NO COMPLETED EXPERIMENT",23,true);
                Panel(18,235,414,160,complete?X13State.equation:"No reaction performed.",20);
                Panel(18,410,414,190,"Simplified educational simulation. Not a quantitative chemical model. Instructor validation pending. No physical experiment performed.",18);
                if(Action(18,630,414,55,"NEW EXPERIMENT",true)){X13State.Reset();Go(3);}
                if(Action(18,700,414,55,"HOME")) Go(1); break;
        }
        GUI.matrix=old;
    }

    void LegacyGUI()
    {
        // Prevent a GUI instance from painting after its scene has been replaced.
        if (!gameObject.scene.IsValid() || gameObject.scene != SceneManager.GetActiveScene()) return;
        if (pendingScene >= 0) return;
        Styles();
        // Scale the entire layout consistently, including font sizes, on portrait windows.
        float scale = Mathf.Min(Screen.width / DesignWidth, Screen.height / DesignHeight);
        scale = Mathf.Max(.1f, scale);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float viewWidth = Screen.width / scale;
        float viewHeight = Screen.height / scale;
        float contentWidth = Mathf.Min(viewWidth - 24f, 540f);
        Rect outer = new Rect((viewWidth - contentWidth) / 2f, 12f, contentWidth, viewHeight - 24f);
        GUILayout.BeginArea(outer);
        scroll = GUILayout.BeginScrollView(scroll, false, false);
        GUILayout.Label("SECTOR X-13", heading, GUILayout.Height(44));
        GUILayout.Label("AR-BASED SAFE CHEMISTRY EXPERIMENTATION SYSTEM", small, GUILayout.Height(36));
        GUILayout.Space(16);
        switch (page)
        {
            case 0:
                GUILayout.Space(160);
                GUILayout.Label("PREVENTIVE SAFETY  •  VIRTUAL EXPERIMENTATION", body, GUILayout.Height(70));
                GUILayout.Space(24);
                if (B("ENTER LABORATORY")) Go(1);
                break;
            case 1:
                GUILayout.Label("HOME DASHBOARD", heading, GUILayout.Height(54));
                GUILayout.Space(15);
                if (B("START EXPERIMENT")) Go(3);
                if (B("CHEMICAL LIBRARY")) Go(2);
                // The lab still enforces its safety lock if entered from Home.
                if (B("LABORATORY")) Go(6);
                break;
            case 2:
                GUILayout.Label("CHEMICAL LIBRARY", heading, GUILayout.Height(54));
                GUILayout.Label("Acids: acetic, hydrochloric, citric, sulfuric.\nBases: sodium, potassium, calcium and magnesium hydroxides.\nAdditional demonstration reagent: sodium bicarbonate.", body, GUILayout.MinHeight(135));
                GUILayout.Label("Chemical safety depends on concentration, quantities and conditions. Detailed SDS records and instructor validation remain pending.", small, GUILayout.MinHeight(90));
                if (B("CHOOSE CHEMICALS")) Go(3);
                if (B("HOME")) Go(1);
                break;
            case 3:
                GUILayout.Label("SELECT TWO CHEMICALS", heading, GUILayout.Height(55));
                GUILayout.Label("BEAKER A — ACID", small, GUILayout.Height(26));
                for (int i = 0; i < X13State.Acids.Length; i++)
                {
                    int index = i;
                    if (B((X13State.acid == index ? "✓ " : "") + X13State.Acids[i])) { X13State.acid = index; X13State.Reset(); }
                }
                GUILayout.Space(12);
                GUILayout.Label("BEAKER B — REAGENT", small, GUILayout.Height(26));
                for (int i = 0; i < X13State.Reagents.Length; i++)
                {
                    int index = i;
                    if (B((X13State.reagent == index ? "✓ " : "") + X13State.Reagents[i])) { X13State.reagent = index; X13State.Reset(); }
                }
                GUILayout.Space(12);
                if (B("ASSESS BEFORE MIXING")) { X13State.Assess(); Go(4); }
                if (B("HOME")) Go(1);
                break;
            case 4:
                if (!X13State.assessed) X13State.Assess();
                GUILayout.Label("PRE-MIX SAFETY ASSESSMENT", heading, GUILayout.Height(70));
                GUILayout.Label("A: " + X13State.Acids[X13State.acid] + "\nB: " + X13State.Reagents[X13State.reagent], body, GUILayout.MinHeight(80));
                GUILayout.Label("RISK: " + X13State.risk, heading, GUILayout.Height(55));
                GUILayout.Label(X13State.note, body, GUILayout.MinHeight(110));
                if (X13State.risk == "MEDIUM" && B("ACKNOWLEDGE VIRTUAL-ONLY PRECAUTIONS")) { X13State.permitted = true; Go(6); }
                if (X13State.risk != "MEDIUM") GUILayout.Label("MIXING BLOCKED — UNREVIEWED PRESET", body, GUILayout.Height(65));
                if (B("SMART SAFETY ADVISOR")) Go(5);
                if (B("CHANGE CHEMICALS")) Go(3);
                break;
            case 5:
                GUILayout.Label("SMART SAFETY ADVISOR", heading, GUILayout.Height(60));
                GUILayout.Label("Never assume dilution alone makes a hazardous reaction safe. Unreviewed combinations remain blocked.", body, GUILayout.MinHeight(110));
                GUILayout.Label("For an educational virtual demonstration, select the predefined dilute HCl + NaOH preset, review its precautions and acknowledge the warning.", body, GUILayout.MinHeight(130));
                if (B("RETURN TO SAFETY")) Go(4);
                if (B("CHANGE CHEMICALS")) Go(3);
                break;
            case 6:
                GUILayout.Label("AR LABORATORY — SIMULATED VIEW", body, GUILayout.Height(50));
                GUILayout.Label("A: " + X13State.Acids[X13State.acid] + "\nB: " + X13State.Reagents[X13State.reagent], small, GUILayout.Height(60));
                GUILayout.Space(250);
                if (!X13State.permitted) GUILayout.Label("SAFETY LOCK ACTIVE — NO MIXING", body, GUILayout.Height(60));
                if (X13State.permitted && B(X13State.reacted ? "REPLAY VIRTUAL REACTION" : "START VIRTUAL REACTION")) { X13State.reacted = true; reactionTime = Time.time; }
                if (X13State.reacted && B("VIEW RESULTS")) Go(7);
                if (B("SAFETY ASSESSMENT")) Go(4);
                if (B("HOME")) Go(1);
                break;
            case 7:
                GUILayout.Label("EXPERIMENT RESULTS", heading, GUILayout.Height(60));
                GUILayout.Label("Risk: " + X13State.risk, body, GUILayout.Height(45));
                GUILayout.Label(X13State.equation, body, GUILayout.MinHeight(80));
                GUILayout.Label(X13State.reacted ? "Simplified virtual demonstration completed. Not a quantitative chemical model." : "No experiment performed.", body, GUILayout.MinHeight(85));
                GUILayout.Label("Instructor validation pending. No physical experiment performed.", small, GUILayout.MinHeight(60));
                if (B("NEW EXPERIMENT")) { X13State.Reset(); Go(3); }
                if (B("HOME")) Go(1);
                break;
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUI.matrix = oldMatrix;
    }

    void Update()
    {
        // Defer loading until after OnGUI finishes; avoids overlapping GUI during transitions.
        if (pendingScene >= 0)
        {
            int target = pendingScene;
            pendingScene = -1;
            SceneManager.LoadScene(Prefix + Names[target]);
            return;
        }
        if (page == 6 && X13State.reacted && beaker1 != null && beaker2 != null)
        {
            float t = Mathf.PingPong((Time.time - reactionTime) * 1.6f, 1f);
            beaker1.transform.localScale = new Vector3(.42f, .33f + .03f * t, .42f);
            beaker2.transform.localScale = new Vector3(.42f, .33f + .03f * (1 - t), .42f);
        }
    }
}
