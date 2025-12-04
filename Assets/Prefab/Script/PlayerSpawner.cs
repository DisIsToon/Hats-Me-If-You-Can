using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform spawnPoint;  // Assign this in the inspector
    public GameObject player;     // Assign your player prefab or object

    void Start()
    {
        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("Player or spawn point not assigned!");
        }
    }
}
