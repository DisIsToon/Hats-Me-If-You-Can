using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HoverStar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("References")]
    public GameObject star;
    public TextMeshProUGUI text;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color32(13, 203, 244, 255); // #0DCBF4

    public void OnPointerEnter(PointerEventData eventData) {
        star.SetActive(true);
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        star.SetActive(false);
        text.color = normalColor;
    }
}