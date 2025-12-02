using UnityEngine;
using UnityEngine.EventSystems;

public class PauseButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Styles")]
    public GameObject style1;
    public GameObject style2;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (style2 != null)
            style2.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (style2 != null)
            style2.SetActive(true);
    }
}
