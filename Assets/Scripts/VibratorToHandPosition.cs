/*───────────────────────────────────────────────────────────────
 * VibratorToHandPosition.cs
 *
 *  他スクリプトが動かすオブジェクトに「揺れ」を付加する。
 *  Z 座標（高さ）が上がるほど振動の振幅と周波数が大きくなる。
 *
 *  使い方
 *  1. Assets ▸ Create ▸ C# Script → “VibratorToHandPosition”
 *  2. 全文貼り付けて保存
 *  3. 振動させたいオブジェクトにアタッチ
 *  4. Inspector で下記パラメータを調整
 *──────────────────────────────────────────────────────────────*/

using UnityEngine;
using Leap;

public class VibratorToHandPosition : MonoBehaviour
{
    /*=========Leap providerの登録==========*/
    [Header("Leap provider (assign the XR/Service Provider)")]
    public LeapProvider leapProvider;   // drag-drop in Inspector
    public Chirality priorityHand = Chirality.Right;

    /*========= 基本振動モード ==========*/
    public enum VibrationMode { None, SinWave, RandomJitter, PerlinNoise }
    [Header("振動モード")]
    public VibrationMode vibration = VibrationMode.SinWave;

    [Header("基準振幅 (m) と基準周波数 (Hz)  ─ y = referenceY のとき")]
    public float baseAmplitude = 0.001f;     // 5 cm
    public float baseFrequency = 5f;        // 5 Hz

    /*========= 振幅の全体上限 ==========*/
    [Header("振幅の最大値 (全モード共通)")]
    public float maxAmplitude = 0.3f;

    /*========= 高さ依存スケール設定 ==========*/
    [Header("高さ依存パラメータ")]
    [Tooltip("振動が増幅し始める基準高さ (m)")]
    public float referenceZ = -0.2f;

    [Tooltip("この高さ差 (m) で振幅・周波数を 2 倍にする")]
    public float gainPerHeight = 12;        // 1 m 上がると 12 倍

    /*========= ランダム／パーリン個別 ==========*/
    public float jitterBaseMax  = 0.02f; // heightFactor 1 のとき
    public float perlinBaseMax  = 0.05f;
    public float perlinSpeed    = 1f;

    /*========= 内部状態 ==========*/
    float t;  // 時間カウンタ
    Vector3 targetPosition;             // ← now private; set every frame

    /*-----------------------------------------------------------*/
    void LateUpdate()
    {
        /* -------------------------------------------------
         * ① Get current Leap frame and pick a hand
         * ------------------------------------------------- */
        Vector3 basePos = transform.position;
        Frame frame = leapProvider.CurrentFrame;
        Hand  hand  = null;

        if (frame != null && frame.Hands.Count > 0)
        {
            // try to grab the requested hand
            hand = frame.Hands.Find(h => h.IsRight && priorityHand == Chirality.Right) ??
                   frame.Hands.Find(h => h.IsLeft  && priorityHand == Chirality.Left)  ??
                   frame.Hands[0];  // fallback: first hand
        }

        /* -------------------------------------------------
         * ② Convert palm position from Leap (mm, rig-space)
         *    →   Unity world-space meters
         * ------------------------------------------------- */
        if (hand != null)
        {
            // Leap units are millimetres; convert to metres then transform
            Vector3 palmLocal  = hand.PalmPosition* 1f; /*Leapmotionとunityのスケールが同じなので変更不要*/
            targetPosition     = leapProvider.transform.TransformPoint(palmLocal);
        }


        /* (1) 手のひらの中心位置 -----------------------*/
        float   heightDelta = Mathf.Max(0f, targetPosition.z - referenceZ); // 下がった分は増幅しない

        /* (2) 高さに応じた倍率を計算 ---------------------------*/
        // 例: heightDelta = gainPerHeight なら 2 倍、それ以上で指数的に増える
        float heightFactor = Mathf.Pow(2f, heightDelta*gainPerHeight);

        /* (3) 振動計算 ----------------------------------------*/
        Vector3 vib = Vector3.zero;
        t += Time.deltaTime;

        switch (vibration)
        {
            case VibrationMode.SinWave:
            {
                float phase = t * baseFrequency * heightFactor * 2f * Mathf.PI;
                float amp   = Mathf.Min(baseAmplitude * heightFactor, maxAmplitude);
                vib = new Vector3(
                          Mathf.Sin(phase),
                          Mathf.Sin(phase + 2f),
                          Mathf.Sin(phase + 4f)) * amp;
                break;
            }
            case VibrationMode.RandomJitter:
            {
                float amp = Mathf.Min(jitterBaseMax * heightFactor, maxAmplitude);
                vib = Random.insideUnitSphere * amp;
                break;
            }
            case VibrationMode.PerlinNoise:
            {
                float amp = Mathf.Min(perlinBaseMax * heightFactor, maxAmplitude);
                vib = new Vector3(
                          Mathf.PerlinNoise(  1f, t * perlinSpeed) - 0.5f,
                          Mathf.PerlinNoise( 10f, t * perlinSpeed) - 0.5f,
                          Mathf.PerlinNoise(100f, t * perlinSpeed) - 0.5f)
                      * (amp * 2f);
                break;
            }
        }

        /* (4) 最終位置を適用 ----------------------------------*/
        transform.position = basePos + vib;
    }
}
