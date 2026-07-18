using System;
using UnityEngine;

public class rabbitobj : MonoBehaviour
{
    [HideInInspector] public GameObject targetBullet;
    [HideInInspector] public Action     onDestroyed;

    public float selfDestroyTime = 3f;

    [Header("Sound Settings")]
    public AudioClip blockSound;

    [Header("Rise Settings")]
    public float startOffsetY  = -1.5f;
    public float targetOffsetY =  0.5f;
    public float riseSpeed     =  50f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool    isRising    = true;
    private bool    isDestroyed = false;   // ← flag กันซ้ำ

    void Start()
    {
        startPos  = new Vector3(transform.position.x, startOffsetY,  transform.position.z);
        targetPos = new Vector3(transform.position.x, targetOffsetY, transform.position.z);
        transform.position = startPos;

        Invoke(nameof(DestroySelf), selfDestroyTime);
    }

    void Update()
    {
        if (isDestroyed) return;   // ← ถ้ากำลังจะถูกทำลายแล้ว ไม่ต้องทำอะไร

        if (isRising)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPos, riseSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isRising           = false;
            }
        }

        // ─── bullet หายไปแล้ว → ทำลายโล่ ───
        if (targetBullet == null)
            DestroySelf();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;   // ← กัน trigger ซ้ำ

        if (other.CompareTag("Bullet"))
        {
            Debug.Log("[Shield] รับกระสุน — ทำลายทั้งคู่!");

            if (blockSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFXAtPoint(blockSound, transform.position);
            }

            Destroy(other.gameObject);
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        if (isDestroyed) return;   // ← กัน DestroySelf ซ้ำ
        isDestroyed = true;

        CancelInvoke();

        try { onDestroyed?.Invoke(); }
        catch { }

        Destroy(gameObject);
    }
}