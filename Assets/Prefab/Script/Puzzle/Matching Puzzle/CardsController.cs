using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardsController : MonoBehaviour, IDataPersistence
{
    public static CardsController Instance { get; set; }

    [Header("Cards")]
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    [Header("UI")]
    public GameObject PuzzleScreen;
    public GameObject completePuzzlePopup;
    public TMP_Text timerText;

    [Header("Timer Settings")]
    public float puzzleDuration = 60f; // Time in seconds
    private float currentTime;
    private bool timerRunning = false;

    [Header("Mirror")]
    public GameObject mirror;
    public GameObject mirrorShardPrefab;

    public bool isOpen;
    private bool canPlayPuzzle = false;

    private List<Sprite> spritePairs;
    Card firstSelected;
    Card secondSelected;
    int matchCounts;

    public bool mirrorClaimed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadData(GameData data)
    {
        this.mirrorClaimed = data.mirrorClaimed;

        if (mirrorClaimed)
        {
            // Destroy the mirror
            if (mirror != null)
                Destroy(mirror);
        }
    }

    public void SaveData(GameData data)
    {
        data.mirrorClaimed = this.mirrorClaimed;
    }


    private void Start()
    {
        PrepareSprites();
        CreateCards();
        PuzzleScreen.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPlayPuzzle && !isOpen)
        {
            SelectionManager.Instance.puzzleDetected = false;
            OpenPuzzle();
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            PuzzleScreenOff();
        }

        // Timer update
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTime).ToString("0"); // Display as whole seconds

            if (currentTime <= 0f)
            {
                timerRunning = false;
                PuzzleScreenOff();
            }
        }
    }

    public void SetCanPlayPuzzle(bool value)
    {
        canPlayPuzzle = value;
    }

    private void OpenPuzzle()
    {
        SoundManager.Instance.PlayPuzzleMusic();
        PuzzleScreen.SetActive(true);
        isOpen = true;

        // Start timer
        currentTime = puzzleDuration;
        timerText.gameObject.SetActive(true);
        timerRunning = true;
    }

    public void PuzzleScreenOff()
    {
        StartCoroutine(ClosePuzzleSequence());
    }

    IEnumerator ClosePuzzleSequence()
    {
        SoundManager.Instance.ReturnToBiomeMusic();
        timerRunning = false;
        timerText.gameObject.SetActive(false);

        // Hide puzzle screen
        PuzzleScreen.SetActive(false);

        // Reset selections
        firstSelected = null;
        secondSelected = null;

        // Optionally, reset cards if you want puzzle to restart
        foreach (Transform t in gridTransform)
        {
            Card c = t.GetComponent<Card>();
            c.Hide();
        }

        matchCounts = 0;
        isOpen = false;

        yield return null;
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        ShuffleSprites(spritePairs);
    }

    private void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);

            // Specify the size you want for this icon
            Vector2 iconSize = new Vector2(60, 80); // change 100,100 to whatever you want
            card.SetIconSprite(spritePairs[i], iconSize);

            card.controller = this;
        }
    }

    public void SetSelected(Card card)
    {
        if (!card.isSelected)
        {
            card.Show();

            if (firstSelected == null)
            {
                firstSelected = card;
                return;
            }

            if (secondSelected == null)
            {
                secondSelected = card;
                StartCoroutine(CheckMatching(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);
        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;
            if (matchCounts >= spritePairs.Count / 2)
            {
                // Puzzle complete
                PuzzleComplete();
            }
        }
        else
        {
            a.Hide();
            b.Hide();
        }
    }

    public void PuzzleComplete()
    {
        canPlayPuzzle = false;
        // Show complete popup
        completePuzzlePopup.SetActive(true);

        // Spawn the mirror shard at mirror's position and rotation
        if (mirrorShardPrefab != null && mirror != null)
        {
            Instantiate(mirrorShardPrefab, mirror.transform.position, mirror.transform.rotation);
        }

        // Destroy the mirror
        if (mirror != null)
            Destroy(mirror);

        // Close the puzzle screen after 1 second
        StartCoroutine(CompleteSequence());
    }


    IEnumerator CompleteSequence()
    {
        yield return new WaitForSeconds(1f);
        PuzzleScreenOff();
        completePuzzlePopup.SetActive(false);
    }

    private void ShuffleSprites(List<Sprite> spriteList)
    {
        for (int i = spriteList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = spriteList[i];
            spriteList[i] = spriteList[randomIndex];
            spriteList[randomIndex] = temp;
        }
    }
}
