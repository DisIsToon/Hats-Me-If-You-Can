using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager1 : MonoBehaviour
{
    // Public list to manually assign targets in the Inspector
    public List<GameObject> targets;

    void Update()
    {
        // Check if all targets are destroyed
        if (AreAllTargetsDestroyed())
        {
            // Start coroutine to transition to the next scene
            StartCoroutine(TransitionToNextScene());
        }
    }

    private bool AreAllTargetsDestroyed()
    {
        // Loop through each target to see if it still exists
        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                return false; // If any target still exists, return false
            }
        }
        return true; // If no targets exist, return true
    }

    private IEnumerator TransitionToNextScene()
    {

        // Wait for 2 seconds
        yield return new WaitForSeconds(2.0f);

        // Load the next scene
        SceneManager.LoadScene("EndingScene1");
    }
}
