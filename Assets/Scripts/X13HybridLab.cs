using UnityEngine;

// Visual-only demonstration layer. The existing X13MultiScene controller owns safety/navigation.
public class X13HybridLab : MonoBehaviour
{
    public Transform leftLiquid, rightLiquid, pourStream, reactionGlow;
    public Transform leftBeaker, rightBeaker;
    public Camera labCamera;
    bool previousReacted;
    float startedAt;

    void Start()
    {
        // Remove the legacy runtime schematic primitives, without touching the scene's saved lab.
        DestroyLegacy("Lab Platform");
        DestroyLegacy("Beaker A (schematic)");
        DestroyLegacy("Beaker B (schematic)");
        if (labCamera != null)
        {
            labCamera.tag = "MainCamera";
            labCamera.transform.position = new Vector3(0f, 1.85f, -4.6f);
            labCamera.transform.LookAt(new Vector3(0f, .65f, 0f));
            labCamera.clearFlags = CameraClearFlags.SolidColor;
            labCamera.backgroundColor = new Color(.015f,.035f,.075f);
        }
        if (pourStream) pourStream.gameObject.SetActive(false);
        if (reactionGlow) reactionGlow.gameObject.SetActive(false);
    }
    void DestroyLegacy(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null && !obj.transform.IsChildOf(transform)) Destroy(obj);
    }
    void Update()
    {
        bool valid = X13State.assessed && X13State.permitted && X13State.risk == "MEDIUM";
        bool reacting = valid && X13State.reacted;
        if (reacting && !previousReacted) startedAt = Time.time;
        previousReacted = reacting;
        float t = reacting ? Time.time - startedAt : 0f;
        if (leftBeaker) leftBeaker.localRotation = Quaternion.identity;
        if (rightBeaker) rightBeaker.localRotation = Quaternion.identity;
        if (leftLiquid) leftLiquid.localScale = new Vector3(.31f, reacting ? Mathf.Lerp(.24f,.06f,Mathf.Clamp01(t/2.5f)) : .24f,.31f);
        if (rightLiquid) rightLiquid.localScale = new Vector3(.31f, reacting ? Mathf.Lerp(.24f,.39f,Mathf.Clamp01(t/2.5f)) : .24f,.31f);
        if (pourStream)
        {
            pourStream.gameObject.SetActive(reacting && t < 2.5f);
            if (pourStream.gameObject.activeSelf)
            {
                pourStream.localScale = new Vector3(.035f,.3f,.035f);
                pourStream.localRotation = Quaternion.Euler(0f,0f,-42f);
            }
        }
        if (reactionGlow)
        {
            reactionGlow.gameObject.SetActive(reacting && t > 1.4f && t < 5.5f);
            if (reactionGlow.gameObject.activeSelf)
            {
                float pulse = 1f + .12f * Mathf.Sin(Time.time*8f);
                reactionGlow.localScale = new Vector3(.35f*pulse,.015f,.35f*pulse);
            }
        }
    }
}
