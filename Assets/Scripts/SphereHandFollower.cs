using UnityEngine;
using Leap;            // core SDK

public class SphereMover : MonoBehaviour
{
    [Header("Leap provider (assign the XR/Service Provider)")]
    public LeapProvider leapProvider;   // drag-drop in Inspector

    [Header("移動速度 (m/s)")]
    public float moveSpeed = 10f;

    // optional: which hand?  0 = first detected, 1 = right, -1 = left …
    public Chirality priorityHand = Chirality.Right;

    Vector3 targetPosition;             // ← now private; set every frame

    void Update()
    {
        /* -------------------------------------------------
         * ① Get current Leap frame and pick a hand
         * ------------------------------------------------- */
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
            Vector3 palmLocal  = hand.PalmPosition* 20f;
            targetPosition     = leapProvider.transform.TransformPoint(palmLocal);
        }

        /* -------------------------------------------------
         * ③ Move sphere toward the hand
         * ------------------------------------------------- */
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);
    }
}