using UnityEngine;

public class VibratorToHandPosition : MonoBehaviour
{
    /* ---------- Hand データ参照 ---------- */
    [Tooltip("HandDataProvider をアタッチ (空なら自動検索)")]
    public HandDataProvider provider;

    /* ---------- 振動モード ---------- */
    public enum VibrationMode { None, SinWave, RandomJitter, PerlinNoise }
    public VibrationMode mode = VibrationMode.SinWave;

    [Header("基準振幅[m] / 周波数[Hz] (Z = referenceZ のとき)")]
    public float baseAmplitude = 0.001f;  // 1 mm
    public float baseFrequency = 5f;       // 5 Hz

    [Header("振幅の最大値 (安全上限)")]
    public float maxAmplitude  = 0.03f;    // 3 cm

    /* ---------- Z 高さ依存 ---------- */
    [Header("高さ依存パラメータ")]
    public float referenceZ     = -0.20f;  // 増幅が始まる高さ
    public float gainPerHeight  = 1f;      // (+1m で 2 倍)

    /* ---------- ランダム / パーリン個別 ---------- */
    public float jitterBaseMax = 0.01f;
    public float perlinBaseMax = 0.02f;
    public float perlinSpeed   = 1f;

    /* ---------- 内部 ---------- */
    Vector3 originalLocal;   // 初期ローカル位置
    float    time;

    void Awake()
    {
        if (!provider) provider = HandDataProvider.Instance;
        originalLocal = transform.localPosition;
    }

    void LateUpdate()
    {
        /* 1. 手が無ければ初期位置に戻し早期リターン */
        if (provider == null || !provider.HandDetected)
        {
            transform.localPosition = originalLocal;
            return;
        }

        /* 2. 高さ差 → 倍率  (2^(Δz/Δh)) */
        float dz = provider.PalmWorldPos.z - referenceZ;
        float factor  = Mathf.Pow(2f, dz*gainPerHeight);

        /* 3. 共通振幅をクリップして計算 */
        float ampBase = Mathf.Min(baseAmplitude * factor, maxAmplitude);

        /* 4. 振動ベクトルを計算 */
        Vector3 vib = Vector3.zero;
        time += Time.deltaTime;

        switch (mode)
        {
            case VibrationMode.SinWave:
            {
                float phase = time * baseFrequency * factor * Mathf.PI * 2f;
                vib = new Vector3(Mathf.Sin(phase),
                                  Mathf.Sin(phase + 2f),
                                  Mathf.Sin(phase + 4f)) * ampBase;
                break;
            }

            case VibrationMode.RandomJitter:
            {
                float amp = Mathf.Min(jitterBaseMax * factor, maxAmplitude);
                vib = Random.insideUnitSphere * amp;
                break;
            }

            case VibrationMode.PerlinNoise:
            {
                float amp = Mathf.Min(perlinBaseMax * factor, maxAmplitude);
                float n   = Mathf.PerlinNoise( 1f, time * perlinSpeed) - 0.5f;
                vib = new Vector3(
                        n,
                        Mathf.PerlinNoise(10f,  time * perlinSpeed) - 0.5f,
                        Mathf.PerlinNoise(100f, time * perlinSpeed) - 0.5f)
                      * (amp * 2f);
                break;
            }
        }

        /* 5. 適用：ローカル原点 + 振動 */
        transform.localPosition = originalLocal + vib;
    }
}
