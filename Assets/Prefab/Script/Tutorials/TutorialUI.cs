using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;

    public void Show(string text)
    {
        tutorialText.text = text;
    }
}
