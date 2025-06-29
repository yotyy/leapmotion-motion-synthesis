using UnityEngine;
using System.Collections;

/// <summary>
/// Adds a local vibration offset to the object.
/// Hand position is supplied by <see cref="HandDataProvider"/>.
/// If the palm is inside the region, it reverses the sphere and background colors.
/// Execution order: after object movement (LateUpdate).
/// </summary>
[DisallowMultipleComponent]
public class ReverseColorWhenPalmInsideTheRegion : MonoBehaviour
{
    public Camera cam;
    public GameObject RegionObject;
    [SerializeField] private HandDataProvider provider;
    public Material SphereMaterial;

    public enum Waveform { None, Sin, Random, Perlin }
    [SerializeField] private Waveform waveform = Waveform.Sin;

    [Header("Waveform parameters")]
    [Min(0)] public float baseFrequency = 10f;
    [Min(0)] public float jitterMax = 0.08f;
    [Min(0)] public float perlinMax = 0.02f;
    [Min(0)] public float perlinSpeed = 1f;
    public float amp = 0.0001f;

    private Color originalBackgroundColor;
    private Color reversedBackgroundColor = Color.white;
    private Color originalSphereColor;
    private Color reversedSphereColor = Color.black;

    private Color  originalEmissionColor;
    private bool   originalEmissionEnabled;

    private Vector3 initLocal;
    private float t;
    private bool hasHand;
    private Vector3 hand_position;

    

    void Awake()
    {
        if (!provider) provider = HandDataProvider.Instance;
        if (!cam)      cam      = Camera.main;

        // ★ここで個別マテリアルを取っておく
        SphereMaterial = GetComponent<Renderer>().material;

        originalBackgroundColor = cam.backgroundColor;
        originalSphereColor     = SphereMaterial.color;      // Standard/Lit 共通
        // URP/Lit なら originalSphereColor = SphereMaterial.GetColor("_BaseColor");
        RegionObject = GameObject.FindWithTag("Region");

        // ────────── Emission の元設定を覚えておく ──────────
        originalEmissionColor   = SphereMaterial.HasProperty("_EmissionColor")
                                ? SphereMaterial.GetColor("_EmissionColor")
                                : Color.black;

        originalEmissionEnabled = SphereMaterial.IsKeywordEnabled("_EMISSION");
        
        initLocal = transform.localPosition;
    }


     void LateUpdate()
    {
        hasHand = provider && provider.IsHandDetected;
        Vector3 hand_position = hasHand ? provider.PalmWorldPos : Vector3.zero;
        
        ReverseColor(hand_position);

        Vector3 vib = CalculateWaveform(amp);

        // ★ 滑らかに振動位置へ近づける
        float smoothSpeed = 5f;
        transform.localPosition = Vector3.Lerp(transform.localPosition, initLocal + vib, Time.deltaTime * smoothSpeed);

        t += Time.deltaTime;
    }

void ReverseColor(Vector3 position)
    {
        bool inside = hasHand && IsPalmInsideTheRegion(position);

        if (inside)
        {
            /* ─── 黒に反転するタイミング ─── */
            cam.backgroundColor = reversedBackgroundColor;

            SphereMaterial.color = reversedSphereColor;     // Albedo = 黒
            // Emission を完全オフ
            SphereMaterial.DisableKeyword("_EMISSION");
        }
        else
        {
            /* ─── 元に戻すタイミング ─── */
            cam.backgroundColor = originalBackgroundColor;

            SphereMaterial.color = originalSphereColor;     // Albedo 元色

            // Emission を元通りに復帰
            if (originalEmissionEnabled)
            {
                SphereMaterial.EnableKeyword("_EMISSION");
                SphereMaterial.SetColor("_EmissionColor", originalEmissionColor);
            }
        }
    }


 public bool IsPalmInsideTheRegion(Vector3 position)
{
    Collider regionCollider = RegionObject.GetComponent<Collider>();
    if (!regionCollider)
    {
        Debug.LogWarning("RegionObject does not have a Collider.");
        return false;
    }

    // ========= デバッグ ==============
    Vector3 worldScaleProvider = provider.transform.lossyScale;
    Vector3 worldScaleRegion   = RegionObject.transform.lossyScale;
    Debug.Log(
        $"PalmWorldPos = {position:F3}\n" +
        $"Provider scale = {worldScaleProvider:F3}\n" +
        $"Region scale   = {worldScaleRegion:F3}\n" +
        $"Region bounds  = {regionCollider.bounds.size:F3}"+
        $"IsInside = {regionCollider.bounds.Contains(position):F3}"
    );
    // =================================

    return regionCollider.bounds.Contains(position);
}


    Vector3 CalculateWaveform(float amp)
    {
        if (amp <= 0f || waveform == Waveform.None) return Vector3.zero;

        switch (waveform)
        {
            case Waveform.Sin:
                float phase = t * baseFrequency * Mathf.PI * 2f;
                return new Vector3(Mathf.Sin(phase),
                                   Mathf.Sin(phase + 2f),
                                   Mathf.Sin(phase + 4f)) * amp;

            case Waveform.Random:
                return Random.insideUnitSphere * Mathf.Min(amp, jitterMax);

            case Waveform.Perlin:
                float n = Mathf.PerlinNoise(1f, t * perlinSpeed) - 0.5f;
                return new Vector3(
                           n,
                           Mathf.PerlinNoise(10f, t * perlinSpeed) - 0.5f,
                           Mathf.PerlinNoise(100f, t * perlinSpeed) - 0.5f)
                       * Mathf.Min(amp, perlinMax) * 2f;

            default:
                return Vector3.zero;
        }
    }
}
