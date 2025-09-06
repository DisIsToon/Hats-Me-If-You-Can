using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage; // Assign in Inspector

    // Add an Item to the slot and make it draggable
    public void AddItem(Item item, Canvas canvas)
    {
        if (iconImage == null || item == null) return;

        iconImage.sprite = item.itemIcon;
        iconImage.enabled = true;

        // Make it draggable
        if (iconImage.gameObject.GetComponent<DraggableItem>() == null)
        {
            DraggableItem drag = iconImage.gameObject.AddComponent<DraggableItem>();
            drag.canvas = canvas;
        }

        // Ensure it has a CanvasGroup for drag
        if (iconImage.gameObject.GetComponent<CanvasGroup>() == null)
        {
            iconImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void ClearSlot()
    {
        if (iconImage == null) return;

        iconImage.sprite = null;
        iconImage.enabled = false;

        // Remove draggable and CanvasGroup if present
        DraggableItem drag = iconImage.gameObject.GetComponent<DraggableItem>();
        if (drag != null) Destroy(drag);

        CanvasGroup cg = iconImage.gameObject.GetComponent<CanvasGroup>();
        if (cg != null) Destroy(cg);
    }
}
