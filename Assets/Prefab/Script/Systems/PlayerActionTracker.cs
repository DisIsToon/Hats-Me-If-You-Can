using UnityEngine;

public class PlayerActionTracker : MonoBehaviour
{
    [Header("Hat Capture Flags")]
    public bool shyHatCaptured = false;
    public bool fastHatCaptured = false;
    public bool jumpHatCaptured = false;

    // Optional: call this to capture a hat
    public void CaptureHat(string hatName)
    {
        switch (hatName)
        {
            case "ShyHat":
                shyHatCaptured = true;
                Debug.Log("ShyHat has been captured!");
                break;
            case "FastHat":
                fastHatCaptured = true;
                Debug.Log("FastHat has been captured!");
                break;
            case "JumpHat":
                jumpHatCaptured = true;
                Debug.Log("JumpHat has been captured!");
                break;
            default:
                Debug.LogWarning("Unknown hat: " + hatName);
                break;
        }
    }

    // Optional: check if all hats are captured
    public bool AllHatsCaptured()
    {
        return shyHatCaptured && fastHatCaptured && jumpHatCaptured;
    }
}