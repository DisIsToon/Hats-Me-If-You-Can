using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public float speed = 2f;
    public float intensity = 2f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float emission = Mathf.PingPong(Time.time * speed, intensity);
        mat.SetColor("_EmissionColor", Color.yellow * emission);
    }
}
