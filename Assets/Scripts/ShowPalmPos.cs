using UnityEngine;
using TMPro;
using Leap;

public class HandPositionDisplay : MonoBehaviour
{
    public LeapProvider leapProvider;
    public TextMeshProUGUI outputText;

    public enum HandChoice { FirstDetected, Left, Right }
    public HandChoice handChoice = HandChoice.Right;

    void Update()
    {
        Frame frame = leapProvider.CurrentFrame;
        Hand  hand  = null;

        if (frame != null && frame.Hands.Count > 0)
        {
            switch (handChoice)
            {
                case HandChoice.Left:
                    hand = frame.Hands.Find(h => h.IsLeft);
                    break;
                case HandChoice.Right:
                    hand = frame.Hands.Find(h => h.IsRight);
                    break;
                case HandChoice.FirstDetected:
                    hand = frame.Hands[0];
                    break;
            }
        }

        if (hand != null)
        {
            Vector3 posM = hand.PalmPosition * 1f;
            outputText.text =
                $"PalmPos (m)\nX: {posM.x:F3}\nY: {posM.y:F3}\nZ: {posM.z:F3}";
        }
        else
        {
            outputText.text = "Hand not found";
        }
    }
}
