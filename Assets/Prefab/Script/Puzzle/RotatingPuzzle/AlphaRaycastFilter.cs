using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaRaycastFilter : MonoBehaviour
{
    [Range(0f, 1f)]
    public float alphaThreshold = 0.1f; // pixels below this alpha won't register clicks

    void Awake()
    {
        Image img = GetComponent<Image>();
        img.alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
