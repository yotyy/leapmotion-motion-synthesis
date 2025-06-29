using UnityEngine;
using Leap;

/// <summary>
///     Centralised, read-only access to the chosen hand’s palm position.<br/>
///     – Updates first every frame (<see cref="DefaultExecutionOrder"/> = -100).<br/>
///     – Singleton (<see cref="Instance"/>) for quick global access.<br/>
///     – No allocations / no GC.
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class HandDataProvider : MonoBehaviour
{
    /* ────────────────────────────
     *  Inspector
     * ──────────────────────────── */

    [Tooltip("XR / Desktop Service Provider prefab")]
    [SerializeField] private LeapProvider leapProvider;

    [Tooltip("Which hand should be tracked")]
    [SerializeField] private Chirality trackedHand = Chirality.Right;

    /* ────────────────────────────
     *  Public read-only API
     * ──────────────────────────── */

    /// <summary> Palm centre in world metres. Returns <c>Vector3.zero</c> if hand missing. </summary>
    public Vector3 PalmWorldPos { get; private set; } = Vector3.zero;

    /// <summary> <c>true</c> while a matching hand is visible this frame. </summary>
    public bool   IsHandDetected { get; private set; }

    /// <summary> Quick global access (null if missing from scene). </summary>
    public static HandDataProvider Instance { get; private set; }

    /* ────────────────────────────
     *  Caching
     * ──────────────────────────── */
    private Transform providerTransform;

    /* ────────────────────────────
     *  Unity lifecycle
     * ──────────────────────────── */

    private void Awake()
    {
        // ─ Singleton enforcement
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!leapProvider)
            leapProvider = FindObjectOfType<LeapProvider>();

        providerTransform = leapProvider ? leapProvider.transform : null;
    }


    private void Update()
    {
        if (!leapProvider)
        {
            SetHandMissing();
            return;
        }

        Frame f = leapProvider.CurrentFrame;
        if (f == null || f.Hands.Count == 0)
        {
            SetHandMissing();
            return;
        }

        Hand hand = GetTrackedHand(f);
        if (hand == null)
        {
            SetHandMissing();
            return;
        }

        UpdatePalmPosition(hand);
    }

    /* ────────────────────────────
     *  Internal helpers
     * ──────────────────────────── */

    private void SetHandMissing()
    {
        IsHandDetected = false;
        PalmWorldPos   = Vector3.zero;
    }

    private Hand GetTrackedHand(Frame frame)
    {
        // Try to find requested chirality, otherwise first hand.
        return frame.Hands.Find(h =>
                    trackedHand == Chirality.Right ? h.IsRight : h.IsLeft)
               ?? frame.Hands[0];
    }

    private void UpdatePalmPosition(Hand hand)
    {
        const float MmToM = 1f;       // ← 修正ポイント
        PalmWorldPos = providerTransform
            ? providerTransform.TransformPoint(hand.PalmPosition * MmToM)
            : Vector3.zero;

        IsHandDetected = true;
    }

}
