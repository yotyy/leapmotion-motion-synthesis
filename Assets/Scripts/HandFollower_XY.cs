using UnityEngine;
public class HandFollower_XY : MonoBehaviour
{
    public HandDataProvider provider;
    public float moveSpeed = 5f;


    void Update()
    {
        if (!provider || !provider.IsHandDetected) return;

        // PalmWorldPos に合わせる
        Vector3 target = provider.PalmWorldPos;

        // Z は固定
        target.z = transform.position.z;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime);
    }
}
