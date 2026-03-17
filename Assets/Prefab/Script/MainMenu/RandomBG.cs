using UnityEngine;
using UnityEngine.UI;

public class RandomBG : MonoBehaviour
{
    [Header("Background Layers")]
    public GameObject L1;
    public GameObject L2;
    public GameObject L3;

    void Start() {
        ShowRandomBG();
    }

    void ShowRandomBG() {
        // Disable BGs 
        L1.SetActive(false);
        L2.SetActive(false);
        L3.SetActive(false);

        int randomIndex = Random.Range(0, 3);

        // Activate the chosen background
        switch (randomIndex) {
            case 0:
                L1.SetActive(true);
                break;
            case 1:
                L2.SetActive(true);
                break;
            case 2:
                L3.SetActive(true);
                break;
        }
    }
}
