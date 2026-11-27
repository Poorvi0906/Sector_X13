using UnityEngine;
using System.Collections.Generic;

// Self-contained desktop simulation. No AR hardware or Unity UI package required.
// Uses runtime IMGUI buttons to avoid persistent UnityEvent callback serialization errors.
public class SectorX13Simulation : MonoBehaviour
{
    enum Page { Splash, Home, Select, Info, Safety, Advisor, Lab, Results }
    Page page=Page.Splash;
    readonly string[] acids={"Acetic Acid (5% vinegar)","Hydrochloric Acid (0.10 M)","Citric Acid","Sulfuric Acid"};
    readonly string[] bases={"Sodium Bicarbonate","Sodium Hydroxide (0.10 M)","Calcium Hydroxide","Magnesium Hydroxide"};
    int a=0,b=0;
    bool assessed=false,allowed=false,played=false;
    string risk="UNKNOWN",explanation="Not assessed",reaction="";
    float reactionStart=-1f;
    GameObject beakerA,beakerB,liquidA,liquidB;
    GUIStyle title,body,button,small;
    Texture2D bg,accent;
    void Start()
    {
        var camera=Camera.main;
        if(camera==null) { var go=new GameObject("Simulation Camera");camera=go.AddComponent<Camera>();go.tag="MainCamera"; }
        camera.transform.position=new Vector3(0,2.1f,-4.5f);
        camera.transform.LookAt(new Vector3(0,.35f,0));
        camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.025f,.045f,.09f);
        camera.fieldOfView=45f;
        if(FindFirstObjectByType<Light>()==null) { var light=new GameObject("Laboratory Light").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(45,-30,0);light.intensity=1.5f; }
        MakeLab();
    }
    Material Material(Color c)
    {
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        if(shader==null)shader=Shader.Find("Standard");
        if(shader==null)shader=Shader.Find("Sprites/Default");
        var m=new Material(shader);m.color=c;return m;
    }
    GameObject Shape(PrimitiveType kind,string name,Vector3 pos,Vector3 scale,Color color,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(kind);go.name=name;go.transform.SetParent(parent);go.transform.position=pos;go.transform.localScale=scale;
        go.GetComponent<Renderer>().material=Material(color);return go;
    }
    void MakeLab()
    {
        Shape(PrimitiveType.Cube,"Laboratory Surface",new Vector3(0,-.09f,0),new Vector3(2.8f,.18f,1.65f),new Color(.11f,.19f,.27f));
        beakerA=MakeBeaker("Beaker A",-.56f,new Color(.04f,.7f,.88f),out liquidA);
        beakerB=MakeBeaker("Beaker B",.56f,new Color(.12f,.86f,.66f),out liquidB);
        ShowLab(false);
    }
    GameObject MakeBeaker(string name,float x,Color tint,out GameObject liquid)
    {
        var parent=new GameObject(name);parent.transform.position=new Vector3(x,0,0);
        Shape(PrimitiveType.Cylinder,"Glass vessel placeholder",new Vector3(x,.32f,0),new Vector3(.39f,.32f,.39f),new Color(.23f,.48f,.58f),parent.transform);
        liquid=Shape(PrimitiveType.Cylinder,"Liquid",new Vector3(x,.20f,0),new Vector3(.34f,.13f,.34f),tint,parent.transform);
        return parent;
    }
    void ShowLab(bool visible)
    {
        foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            if(r.gameObject.name=="Laboratory Surface" || r.transform.IsChildOf(beakerA.transform) || r.transform.IsChildOf(beakerB.transform))r.enabled=visible;
    }
    void ChooseA(int i){a=i;ResetAssessment();}
    void ChooseB(int i){b=i;ResetAssessment();}
    void ResetAssessment(){assessed=false;allowed=false;played=false;risk="UNKNOWN";explanation="Assess this exact pair before mixing.";reaction="";}
    void Assess()
    {
        assessed=true;played=false;allowed=false;
        if(a==0 && b==0)
        {
            risk="UNKNOWN";
            explanation="EXP-01: Vinegar + bicarbonate. Educational candidate only; exact preset awaits instructor validation. CO2 pressure risk in sealed vessels.";
            reaction="CH3COOH + NaHCO3 -> CH3COONa + H2O + CO2";
        }
        else if(a==1 && b==1)
        {
            risk="MEDIUM";
            explanation="EXP-02: Dilute acid/base neutralisation. Exothermic. Virtual-only demonstration requires explicit acknowledgement; never extrapolate to concentrated stocks.";
            reaction="HCl + NaOH -> NaCl + H2O";
        }
        else
        {
            risk="UNKNOWN";
            explanation="No verified concentration-specific experiment for this combination. Mixing blocked.";
            reaction="No validated equation for selected conditions.";
        }
    }
    void InitStyles()
    {
        if(title!=null)return;
        title=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};
        title.normal.textColor=new Color(.05f,.94f,1f);
        body=new GUIStyle(GUI.skin.label){fontSize=17,alignment=TextAnchor.MiddleCenter,wordWrap=true};
        body.normal.textColor=Color.white;
        small=new GUIStyle(body){fontSize=13};
        button=new GUIStyle(GUI.skin.button){fontSize=16,fontStyle=FontStyle.Bold,wordWrap=true};
        button.normal.textColor=Color.white;
    }
    bool Btn(string text){return GUILayout.Button(text,button,GUILayout.MinHeight(46));}
    void OnGUI()
    {
        InitStyles();
        float width=Mathf.Min(Screen.width-24,850);
        GUILayout.BeginArea(new Rect((Screen.width-width)/2,12,width,Screen.height-24));
        GUILayout.Label("SECTOR X-13",title,GUILayout.Height(44));
        GUILayout.Label("AR-BASED SAFE CHEMISTRY EXPERIMENTATION SYSTEM",small);
        GUILayout.Space(12);
        switch(page)
        {
            case Page.Splash:
                GUILayout.FlexibleSpace();GUILayout.Label("VIRTUAL CHEMISTRY • PREVENTIVE SAFETY • EXPERIMENT ANALYSIS",body);
                GUILayout.Space(24);if(Btn("ENTER SECTOR X-13"))page=Page.Home;GUILayout.FlexibleSpace();break;
            case Page.Home:
                GUILayout.Label("Welcome to the laboratory",body);GUILayout.Space(18);
                if(Btn("START EXPERIMENT"))page=Page.Select;
                if(Btn("EXPLORE CHEMICAL INFORMATION"))page=Page.Info;
                if(Btn("OPEN LABORATORY SIMULATION")){page=Page.Lab;ShowLab(true);}
                GUILayout.FlexibleSpace();break;
            case Page.Select:
                GUILayout.Label("SELECT EXACTLY TWO CHEMICALS",body);
                GUILayout.Label("BEAKER A — ACID",small);
                for(int i=0;i<acids.Length;i++)if(Btn((a==i?"[SELECTED] ":"")+acids[i]))ChooseA(i);
                GUILayout.Label("BEAKER B — BASE / REAGENT",small);
                for(int i=0;i<bases.Length;i++)if(Btn((b==i?"[SELECTED] ":"")+bases[i]))ChooseB(i);
                if(Btn("CONTINUE TO SAFETY ANALYSIS")){Assess();page=Page.Safety;}
                break;
            case Page.Info:
                GUILayout.Label("CHEMICAL INFORMATION",body);
                GUILayout.Label("A: "+acids[a]+"\nB: "+bases[b],body);
                GUILayout.Label("Information is illustrative. SDS, concentration, incompatibilities and controls must be checked before physical use.",body);
                if(Btn("SELECT CHEMICALS"))page=Page.Select;break;
            case Page.Safety:
                GUILayout.Label("PRE-MIX SAFETY ASSESSMENT",body);
                GUILayout.Label("A: "+acids[a]+"\nB: "+bases[b],body);
                GUILayout.Label("RISK: "+risk, title);
                GUILayout.Label(explanation,body);
                if(!assessed && Btn("ASSESS"))Assess();
                if(risk=="MEDIUM" && Btn("ACKNOWLEDGE VIRTUAL-ONLY PRECAUTIONS")){allowed=true;page=Page.Lab;ShowLab(true);}
                if(risk!="MEDIUM")GUILayout.Label("MIXING BLOCKED — preset has not been validated.",body);
                if(Btn("SMART SAFETY ADVISOR"))page=Page.Advisor;
                if(Btn("CHANGE CHEMICALS"))page=Page.Select;break;
            case Page.Advisor:
                GUILayout.Label("SMART SAFETY ADVISOR",title);
                GUILayout.Label("The selected scenario is not automatically made safe by dilution. Use a reviewed preset with exact quantities and controls.",body);
                GUILayout.Label("Educational alternative: explore the already defined dilute HCl + NaOH virtual preset. It remains MEDIUM-risk and requires precautions.",body);
                if(Btn("RETURN TO SAFETY ANALYSIS"))page=Page.Safety;
                if(Btn("SELECT CHEMICALS"))page=Page.Select;break;
            case Page.Lab:
                GUILayout.Label("AR LABORATORY — SIMULATED ENVIRONMENT",body);
                GUILayout.Label("A: "+acids[a]+"     |     B: "+bases[b],small);
                GUILayout.FlexibleSpace();
                if(!allowed)GUILayout.Label("PREVENTIVE SAFETY LOCK ACTIVE. Mixing is disabled.",body);
                if(allowed && Btn(played?"REPLAY VIRTUAL REACTION":"START VIRTUAL EXPERIMENT")){played=true;reactionStart=Time.time;}
                if(Btn("SAFETY ANALYSIS")){page=Page.Safety;ShowLab(false);}
                if(played && Btn("VIEW RESULTS")){page=Page.Results;ShowLab(false);}
                if(Btn("HOME")){page=Page.Home;ShowLab(false);}
                break;
            case Page.Results:
                GUILayout.Label("EXPERIMENT RESULTS",title);
                GUILayout.Label("Classification: "+risk,body);
                GUILayout.Label(reaction,body);
                GUILayout.Label(played?"Simplified virtual neutralisation demonstration; not a quantitative prediction.":"No approved reaction was performed.",body);
                GUILayout.Label("No physical experiment was conducted. Instructor review remains necessary.",small);
                if(Btn("NEW EXPERIMENT")){ResetAssessment();page=Page.Select;}break;
        }
        GUILayout.EndArea();
    }
    void Update()
    {
        if(played && liquidA!=null && liquidB!=null)
        {
            float t=Mathf.PingPong((Time.time-reactionStart)*2f,1f);
            liquidA.transform.localScale=new Vector3(.34f,.13f+.035f*t,.34f);
            liquidB.transform.localScale=new Vector3(.34f,.13f+.035f*(1f-t),.34f);
        }
    }
}
