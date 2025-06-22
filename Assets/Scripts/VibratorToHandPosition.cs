using UnityEngine;

/// <summary>
///  Adds a local “vibration offset” to the object.<br/>
///  * Hand position is supplied by <see cref="HandDataProvider"/>.<br/>
///  * Amplitude is modulated by Z-height according to one of 3 profiles.<br/>
///  * Waveform (Sin / Random / Perlin) is independent of amplitude profile.<br/>
///  Execution order: after the object has moved (LateUpdate).
/// </summary>
[DisallowMultipleComponent]
public sealed class VibratorToHandPosition : MonoBehaviour
{
    /* ───────────────────────────────── 1. Dependencies ───────────────────────────────── */

    [SerializeField] private HandDataProvider provider;

    /* ──────────────────────────────── 2. Waveform mode ──────────────────────────────── */

    public enum Waveform { None, Sin, Random, Perlin }
    [SerializeField] private Waveform waveform = Waveform.Sin;

    [Header("Waveform parameters")]
    [Min(0)] public float baseFrequency = 5f;     // Sin [Hz]
    [Min(0)] public float jitterMax     = 0.01f;  // Random max amp
    [Min(0)] public float perlinMax     = 0.02f;  // Perlin max amp
    [Min(0)] public float perlinSpeed   = 1f;

    /* ────────────────────────────── 3. Amplitude profile ────────────────────────────── */

    public enum AmpProfile { Peak, Plateau, Ramp }
    [SerializeField] private AmpProfile profile = AmpProfile.Peak;

    [Header("Peak profile  ( ▲ shape )")]
    public float peakZ      =  0.1f;   // height of the maximum
    public float peakWidth  =  0.2f;   // FWHM of the bell (half–width)

    [Header("Plateau profile  ( ▄ shape )")]
    public float plateauMinZ = -0.05f; // start Z of flat-top
    public float plateauMaxZ =  0.15f; // end Z of flat-top
    public float plateauAmpGain  =  0.03f; // flat amplitude

    [Header("Ramp profile  ( ／ shape )")]
    public float rampRefZ   = -0.2f;   // zero point
    public float rampGain   =  10f;     // amp = base * 2^(Δz / gain)

    [Header("Common limits")]
    [Min(0)] public float baseAmplitude = 0.001f; // starting amp
    [Min(0)] public float ampMax        = 0.03f;  // safety cap

    /* ─────────────────────────────── 4. Internal state ─────────────────────────────── */

    Vector3 initLocal;
    float   t;

    /* ──────────────────────────────────────── */

    void Awake()
    {
        if (!provider) provider = HandDataProvider.Instance;
        initLocal = transform.localPosition;
    }

    void LateUpdate()
    {
        if (!provider || !provider.IsHandDetected)
        {
            transform.localPosition = initLocal;
            return;
        }

        float ampFactor = GetAmplitudeFactor(provider.PalmWorldPos.z);
        float amp       = Mathf.Clamp(baseAmplitude * ampFactor, 0f, ampMax);
        Debug.Log(amp);

        Vector3 vib = CalculateWaveform(amp);

        transform.localPosition = initLocal + vib;
        t += Time.deltaTime;
    }

    /* ─────────────────────────────── Helpers ─────────────────────────────── */

    float GetAmplitudeFactor(float z)
    {
        switch (profile)
        {
            /* ▲: rises to a single peak then falls */
            case AmpProfile.Peak:
            {
                float d = Mathf.Abs(z - peakZ);
                return Mathf.Pow(2f, 1/d);                // linear falloff
            }

            /* ▄: flat top between min & max, zero outside */
            case AmpProfile.Plateau:
            {
                return (z >= plateauMinZ && z <= plateauMaxZ) ? Mathf.Pow(2f, plateauAmpGain * baseAmplitude) : 1f;
            }

            /* ／: exponential ramp after ref-height */
            case AmpProfile.Ramp:
            default:
            {
                float dz = z - rampRefZ;
                return dz <= 0 ? 0f : Mathf.Pow(2f, dz * rampGain);
            }
        }
    }

    Vector3 CalculateWaveform(float amp)
    {
        if (amp <= 0f || waveform == Waveform.None) return Vector3.zero;

        switch (waveform)
        {
            case Waveform.Sin:
            {
                float phase = t * baseFrequency * Mathf.PI * 2f;
                return new Vector3(Mathf.Sin(phase),
                                   Mathf.Sin(phase + 2f),
                                   Mathf.Sin(phase + 4f)) * amp;
            }
            case Waveform.Random:
                return Random.insideUnitSphere * Mathf.Min(amp, jitterMax);

            case Waveform.Perlin:
            {
                float n = Mathf.PerlinNoise(1f, t * perlinSpeed) - 0.5f;
                return new Vector3(
                           n,
                           Mathf.PerlinNoise(10f , t * perlinSpeed) - 0.5f,
                           Mathf.PerlinNoise(100f, t * perlinSpeed) - 0.5f)
                       * Mathf.Min(amp, perlinMax) * 2f;
            }
            default:
                return Vector3.zero;
        }
    }
}
