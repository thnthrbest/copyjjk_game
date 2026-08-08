using UnityEngine;

public class PointPickup : MonoBehaviour
{
    [Header("Point Settings")]
    public int pointAmount = 1;

    [Header("Pickup VFX & Sound")]
    public GameObject pickupVFX;
    public AudioClip pickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // บันทึกแต้มสะสมลง PlayerPrefs
            PlayerStatsManager.AddPoints(pointAmount);

            // เล่นเสียง
            if (pickupSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXAtPoint(pickupSound, transform.position);
            }

            // แสดง Effect ตอนเก็บ
            if (pickupVFX != null)
            {
                Instantiate(pickupVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
