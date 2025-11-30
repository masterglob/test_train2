using UnityEngine;
using UnityEngine.UI;

public class UIIndicator : MonoBehaviour
{
    public Color directColor = Color.green;
    public Color indirectColor = Color.red;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetState(bool state)
    {
        img.color = state ? directColor : indirectColor;
    }
}
