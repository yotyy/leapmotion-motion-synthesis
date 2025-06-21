using UnityEngine;

/// <summary>
/// 10×10 の球を XY 平面に 5 m 間隔で並べるスクリプト。（gridSize と spacing を変えれば汎用）
/// </summary>
public class SphereDuplicater : MonoBehaviour
{
    [Header("複製する Sphere のプレハブ")]
    public GameObject spherePrefab;

    [Header("格子サイズ (N×N)")]
    [Min(1)] public int gridSize = 20;

    [Header("等間隔 (m)")]
    public float spacing = 5f;

    void Awake()   // Play 直後に 1 回だけ実行
    {
        if (!spherePrefab)
        {
            Debug.LogError("spherePrefab が設定されていません");
            return;
        }

        // グリッド中央が親の位置になるようオフセット
        float offset = (gridSize - 1) * spacing * 0.5f;

        for (int y = 0; y < gridSize; y++)
        for (int x = 0; x < gridSize; x++)
        {
            Vector3 localPos = new Vector3(x * spacing - offset,
                                           y * spacing - offset,
                                           0f);               // Z=0 の XY 平面

            // ワールド座標 = 親の位置 + オフセット
            Instantiate(spherePrefab,
                        transform.position + localPos,
                        Quaternion.identity,
                        transform);           // 親を this にすると階層が整理される
        }
    }
}
