using UnityEngine;
using TMPro;

public class SwIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh = null;

    public string Text
    {
        get => textMesh.text;
        set => textMesh.text = value;
    }

    private void Awake()
    {
            textMesh = GetComponentInChildren<TextMeshPro>();
    }
}
