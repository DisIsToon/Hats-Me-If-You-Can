using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsController : MonoBehaviour
{
    public static CardsController Instance { get; set; }

    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    public GameObject PuzzleScreen;
    public GameObject completePuzzlePopup;

    public bool isOpen;

    private List<Sprite> spritePairs;

    Card firstSelected;
    Card secondSelected;

    int matchCounts;

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

    private void Start()
    {
        PrepareSprites();
        CreateCards();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && !isOpen)
        {
            PuzzleScreen.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.O) && isOpen)
        {
            PuzzleScreen.SetActive(false);

            isOpen = false;
        }
    }
    private void PrepareSprites()
    {
        spritePairs = new List<Sprite> ();
        for (int i = 0; i < sprites.Length; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.controller = this;
        }
    }

    public void SetSelected(Card card)
    {
        if (card.isSelected == false)
        {
            card.Show();

            if(firstSelected == null)
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
        if(a.iconSprite == b.iconSprite)
        {
            // Matched
            matchCounts++;
            if (matchCounts >= spritePairs.Count / 2)
            {
                // level complete
                completePuzzlePopup.SetActive(true);
            }
        }
        else
        {
            // flip back
            a.Hide();
            b.Hide();
        }
    }

    void ShuffleSprites(List<Sprite> spriteList)
    {
        for (int i = spriteList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            //Swap the element at i and random index
            Sprite temp = spriteList[i];
            spriteList[i] = spriteList[randomIndex];
            spriteList[randomIndex] = temp; 
        }
    }
}
