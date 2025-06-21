using UnityEngine;
using Leap;

public class HandFollower_XY : MonoBehaviour
{
    [Header("Leap provider (Service Provider)")]
    public LeapProvider leapProvider;

    [Header("移動速度 (m/s)")]
    public float moveSpeed = 10f;

    public Chirality priorityHand = Chirality.Right;

    Vector3 targetPos;

    void Update()
    {
        /* ① 手を取得 ------------------------------------------------ */
        if (leapProvider == null) return;

        Frame frame = leapProvider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0) return;

        Hand hand = frame.Hands.Find(h => priorityHand == Chirality.Right ? h.IsRight : h.IsLeft)
                   ?? frame.Hands[0];

        /* ② ワールド座標へ変換 (mm→m) ------------------------------ */
        Vector3 palmWorld = leapProvider.transform.TransformPoint(hand.PalmPosition * 10f);

        /* ③ Z だけ固定して XY だけ反映 ------------------------------ */
        targetPos = new Vector3(palmWorld.x, palmWorld.y, 0);

        /* ④ 移動 --------------------------------------------------- */
        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 moveSpeed * Time.deltaTime);
    }
}
