using UnityEngine;

public class VibrationEffect : MonoBehaviour
{
    public enum Mode { Sin, Random, Perlin }
    public Mode mode = Mode.Sin;

    public float amplitude = 0.001f;   // 基準振幅
    public float frequency = 5f;      // Sin 用
    public float randomMax = 0.02f;   // Random 用
    public float perlinSpeed = 1f;    // Perlin 用
    public float maxAmplitude = 0.2f; // 上限

    Vector3 baseLocal; float t;

    void Start()  => baseLocal = transform.localPosition;

    void LateUpdate()
    {
        t += Time.deltaTime;
        Vector3 vib = Vector3.zero;

        switch (mode)
        {
            case Mode.Sin:
                float ph = t * frequency * Mathf.PI * 2f;
                vib = new Vector3(Mathf.Sin(ph), Mathf.Sin(ph + 2f), Mathf.Sin(ph + 4f))
                      * Mathf.Min(amplitude, maxAmplitude);
                break;

            case Mode.Random:
                vib = Random.insideUnitSphere * Mathf.Min(randomMax, maxAmplitude);
                break;

            case Mode.Perlin:
                float n = Mathf.PerlinNoise(1f, t * perlinSpeed) - 0.5f;
                vib = new Vector3(n,
                                  Mathf.PerlinNoise(10f, t * perlinSpeed) - 0.5f,
                                  Mathf.PerlinNoise(100f, t * perlinSpeed) - 0.5f)
                      * maxAmplitude;
                break;
        }
        transform.localPosition = baseLocal + vib;
    }
}
