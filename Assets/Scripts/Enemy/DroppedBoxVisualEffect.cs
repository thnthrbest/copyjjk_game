using UnityEngine;

/// <summary>
/// Visual effect for 3D Box dropped when an enemy dies.
/// Gently rotates and floats upward over its lifespan before disappearing.
/// </summary>
public class DroppedBoxVisualEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 0.8f;
    public float rotationSpeed = 120.0f;

    void Update()
    {
        // Gentle Y-axis rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Gentle upward movement
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
    }
}
