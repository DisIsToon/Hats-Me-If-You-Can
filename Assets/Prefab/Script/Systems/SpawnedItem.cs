using UnityEngine;

public class SpawnedItem : MonoBehaviour
{
    private ItemSpawnTrigger currentTrigger;

    public void SetTrigger(ItemSpawnTrigger trigger)
    {
        currentTrigger = trigger;
    }

    private void OnDestroy()
    {
        if (currentTrigger != null)
        {
            currentTrigger.NotifyItemRemoved();
        }
    }
}
