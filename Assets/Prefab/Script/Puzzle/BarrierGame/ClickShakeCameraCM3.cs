using UnityEngine;
using Unity.Cinemachine;

public class ClickShakeCameraCM3 : MonoBehaviour
{
    public float shakeAmplitude = 2f;
    public float shakeFrequency = 2f;
    public float shakeDuration = 0.15f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;

    void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        if (noise == null) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0f)
            {
                noise.AmplitudeGain = 0f;
                noise.FrequencyGain = 0f;
            }
        }
    }

    public void ShakeCamera()
    {
        if (noise == null) return;

        noise.AmplitudeGain = shakeAmplitude;
        noise.FrequencyGain = shakeFrequency;
        shakeTimer = shakeDuration;
    }
}