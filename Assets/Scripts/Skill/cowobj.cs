using UnityEngine;

public class cowobj : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 25f;       // Rushing speed of the cow
    public float lifetime = 5f;     // Cow object lifetime (5 seconds before disappearing)

    [Header("Sound Settings")]
    public AudioClip crashSound;

    private Transform playerTransform;

    void Start()
    {
        // Destroy this cow object automatically after 5 seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Charge forward in its forward direction
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void Setup(Transform player)
    {
        playerTransform = player;
    }

    // =====================================
    // ชนแล้วทำลายสิ่งกีดขวาง (ไม่ทำลายตัวเอง)
    // =====================================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log($"[CowObj] พุ่งชนทำลายสิ่งกีดขวาง: {other.gameObject.name}");

            if (crashSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXAtPoint(crashSound, transform.position);
            }

            Destroy(other.gameObject);
        }
    }
}
