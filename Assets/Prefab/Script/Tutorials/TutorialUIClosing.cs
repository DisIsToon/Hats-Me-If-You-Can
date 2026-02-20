using UnityEngine;

public class TutorialUIClosing : MonoBehaviour
{
    [Header("Target To Close")]
    public GameObject targetObject;

    public void CloseObject()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No target object assigned to CloseGameObjectOnClick.");
        }
    }
}
