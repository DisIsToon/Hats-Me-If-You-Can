using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DelayedPopup : MonoBehaviour
{
    public GameObject popupScreen;
    public GameObject popupScreen2;
    public float delay = 1f;

    void Start()
    {
        popupScreen.SetActive(false);
        popupScreen2.SetActive(false);
        StartCoroutine(ShowPopupAfterDelay());
    }

    IEnumerator ShowPopupAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        popupScreen.SetActive(true);
        popupScreen2.SetActive(true);

    }
}
