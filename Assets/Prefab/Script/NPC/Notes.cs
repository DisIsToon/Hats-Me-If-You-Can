using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Notes : MonoBehaviour
{
    [Header("Note Settings")]
    [TextArea(3, 8)]
    public string noteText;

    public GameObject pressFUI;
    public GameObject noteScreen;   // ⬅ your per-note screen (speaker, text, button)

    public TMP_Text dialogText;
    public TMP_Text speakerText;
    public Button option1BTN;

    public bool activeNote;
    private bool playerInRange = false;

    public static List<Notes> allNotes = new List<Notes>();
    public static Notes Instance { get; private set; }

    private void Awake()
    {
        if (!allNotes.Contains(this))
            allNotes.Add(this);
    }

    private void OnDestroy()
    {
        if (allNotes.Contains(this))
            allNotes.Remove(this);

        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        if (pressFUI != null)
            pressFUI.SetActive(false);

        if (noteScreen != null)
            noteScreen.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (pressFUI != null && (noteScreen == null || !noteScreen.activeSelf))
            pressFUI.SetActive(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenNote();
        }
    }

    // ----------------------------------------------------
    // OPEN NOTE
    // ----------------------------------------------------
    public void OpenNote()
    {
        Instance = this;
        activeNote = true;

        if (pressFUI != null)
            pressFUI.SetActive(false);

        if (noteScreen != null)
            noteScreen.SetActive(true);

        if (dialogText != null)
            dialogText.text = noteText;

        if (speakerText != null)
            speakerText.text = "Note";

        // Button setup
        if (option1BTN != null)
        {
            option1BTN.gameObject.SetActive(true);

            var t = option1BTN.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null) t.text = "Close";

            option1BTN.onClick.RemoveAllListeners();
            option1BTN.onClick.AddListener(CloseNote);
        }
    }

    // ----------------------------------------------------
    // CLOSE NOTE
    // ----------------------------------------------------
    public void CloseDialog()
    {
        CloseNote();
    }

    public void CloseNote()
    {
        if (Instance == this)
            Instance = null;

        activeNote = false;

        if (noteScreen != null)
            noteScreen.SetActive(false);
    }

    // ----------------------------------------------------
    // TRIGGERS
    // ----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (pressFUI != null)
                pressFUI.SetActive(false);

            if (noteScreen != null && noteScreen.activeSelf)
                if (Instance == this)
                    CloseNote();
        }
    }
}
