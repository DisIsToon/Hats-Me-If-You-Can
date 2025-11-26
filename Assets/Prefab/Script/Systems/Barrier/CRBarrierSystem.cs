using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class CRBarrierSystem : MonoBehaviour
{
    [Header("Message Object Settings")]
    public GameObject messageObject;      // Message when ShyHat not captured
    public GameObject messageObject2;     // Message when ShyHat captured
    public float fadeDuration = 1f;
    public float showTime = 2f;

    [Header("Mini Game")]
    public GameObject barrierGameScreen;  // Assign your BarrierGameScreen (ClickerBarrier root)
    public ClickerBarrier clickerGame; // Reference to ClickerBarrier script

    private bool isShowing = false;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;
    private GameTracker gt;

    private void Start()
    {
        gt = FindObjectOfType<GameTracker>();
        if (gt == null)
        {
            Debug.LogError("GameTracker not found!");
        }

        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (messageObject) messageObject.SetActive(false);
        if (messageObject2) messageObject2.SetActive(false);
        if (barrierGameScreen) barrierGameScreen.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isShowing) return;

        /*if (gt != null && gt.jumpHatCaptured)
        {
            // Show message 2 then start mini game
            StartCoroutine(ShowAndStartGame(messageObject2));
        }*/
        if (gt != null && gt.IsPuzzleComplete())
        {
            StartCoroutine(ShowAndStartGame(messageObject2));
        }
        else
        {
            // Just show regular message
            StartCoroutine(ShowAndFade(messageObject));
        }
    }

    private IEnumerator ShowAndFade(GameObject msgObj)
    {
        if (msgObj == null) yield break;
        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        float elapsed = 0f;
        Image img = msgObj.GetComponent<Image>();
        SpriteRenderer sr = msgObj.GetComponent<SpriteRenderer>();

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            if (img)
            {
                Color c = img.color; c.a = alpha; img.color = c;
            }
            else if (sr)
            {
                Color c = sr.color; c.a = alpha; sr.color = c;
            }

            yield return null;
        }

        msgObj.SetActive(false);
        isShowing = false;
    }

    private IEnumerator ShowAndStartGame(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        // Hide message and open the Clicker mini-game
        msgObj.SetActive(false);
        isShowing = false;


        if (clickerGame == null)
        {
            Debug.Log("Clicker Game is null");
        }
        if (barrierGameScreen != null)
        {
            barrierGameScreen.SetActive(true);
        }

        if (clickerGame != null)
        {
            clickerGame.BarrierGameScreen = barrierGameScreen;
            clickerGame.StartGame(); // <---- Start the mini-game
            StartCoroutine(WaitForVictory());
        }
    }

    private IEnumerator WaitForVictory()
    {
        // Wait until ClickerBarrierTMP sets its gameEnded = true
        while (!clickerGame.gameEnded)
        {
            yield return null;
        }

        // When ended, check if victory
        if (clickerGame.victoryScreen.activeSelf)
        {
            // Disable barrier visuals and collision
            if (boxCollider) boxCollider.enabled = false;
            if (meshRenderer) meshRenderer.enabled = false;
        }
    }
}

