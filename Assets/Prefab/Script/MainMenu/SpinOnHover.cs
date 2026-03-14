using UnityEngine;
using UnityEngine.EventSystems;

public class SpinOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;

    public void OnPointerEnter(PointerEventData eventData) {
        animator.SetTrigger("Spin");
    }

    public void OnPointerExit(PointerEventData eventData) {
        animator.Play("Idle");
    }
}
