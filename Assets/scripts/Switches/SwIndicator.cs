using UnityEngine;
using TMPro;

public class SwIndicator : MonoBehaviour
{
    private TextMeshPro textMesh = null;
    private bool isDirect = true;
    public bool divergingOnRight = true;
    public UIIndicator uiIndicator = null;


    public bool IsDirect() { return isDirect; }

    public string Text
    {
        get => textMesh.text;
        set => textMesh.text = value;
    }

    public void SetDirect(bool direct)
    {
        isDirect = direct;
        if (direct)
        {
            textMesh.text = "^";
        }
        else if (divergingOnRight)
        {
            textMesh.text = ">";
        }
        else
        {
            textMesh.text = "<";
        }

        if (uiIndicator != null)
        {
            uiIndicator.SetState(isDirect);
        }
    }

    private void Awake()
    {
            textMesh = GetComponentInChildren<TextMeshPro>();
    }
}
