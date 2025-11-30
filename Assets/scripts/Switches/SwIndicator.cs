using UnityEngine;
using TMPro;

public class SwIndicator : MonoBehaviour
{
    private TextMeshPro textMesh = null;
    public bool divergingOnRight = true;

    public string Text
    {
        get => textMesh.text;
        set => textMesh.text = value;
    }

    public void SetDirect(bool direct)
    {
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
    }

    private void Awake()
    {
            textMesh = GetComponentInChildren<TextMeshPro>();
    }
}
