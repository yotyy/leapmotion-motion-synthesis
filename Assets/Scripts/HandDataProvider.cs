using UnityEngine;
using Leap;

[DefaultExecutionOrder(-100)]               // どの MonoBehaviour より先に走らせる
public class HandDataProvider : MonoBehaviour
{
    [Tooltip("Service Provider (Desktop / XR) をドラッグ")]
    public LeapProvider leapProvider;

    [Tooltip("追従したい手 (Right / Left)")]
    public Chirality whichHand = Chirality.Right;

    /* ======= 外部から読み取れるプロパティ ======= */
    public Vector3 PalmWorldPos { get; private set; } = Vector3.zero;
    public bool HandDetected { get; private set; }

    /* ======= 他スクリプトが直接参照しやすいようシングルトン化 ======= */
    public static HandDataProvider Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);     // 重複を防ぐ
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (leapProvider == null) { HandDetected = false; return; }

        Frame frame = leapProvider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0)
        {
            HandDetected = false;
            return;
        }

        Hand hand = frame.Hands.Find(h =>
                     whichHand == Chirality.Right ? h.IsRight : h.IsLeft)
                   ?? frame.Hands[0];

        PalmWorldPos = leapProvider.transform.TransformPoint(hand.PalmPosition * 1f);
        HandDetected = true;
    }
}
