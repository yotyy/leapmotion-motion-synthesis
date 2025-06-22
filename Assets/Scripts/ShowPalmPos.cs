using UnityEngine;
using TMPro;

/// <summary>
/// HandDataProvider が更新した palm 座標を HUD に表示するだけの軽量スクリプト
/// </summary>
public class HandPositionDisplay : MonoBehaviour
{
    [Tooltip("対象となる HandDataProvider (Left / Right など)")]
    public HandDataProvider provider;          // ← Inspector でドラッグ

    [Tooltip("表示先 TextMeshPro (UGUI)")]
    public TextMeshProUGUI  outputText;

    void Awake()
    {
        // provider が設定されていない場合はシーンから最初のものを取得
        if (!provider)
            provider = FindObjectOfType<HandDataProvider>();
    }

    void Update()
    {
        if (!provider || !outputText) return;

        if (provider.IsHandDetected)
        {
            Vector3 p = provider.PalmWorldPos;     // すでに m 単位
            outputText.text =
                $"Palm Pos (m)\n" +
                $"X: {p.x:F3}\n" +
                $"Y: {p.y:F3}\n" +
                $"Z: {p.z:F3}";
        }
        else
        {
            outputText.text = "Hand not found";
        }
    }
}
