using System;     
using UnityEngine;

public class rabbitobj : MonoBehaviour
{
   [HideInInspector] public GameObject targetBullet;
    [HideInInspector] public Action onDestroyed;   // ← callback

    public float selfDestroyTime  = 3f;

    [Header("Rise Settings")]
    public float startOffsetY  = -1.5f;
    public float targetOffsetY =  0.5f;
    public float riseSpeed     =  50f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool    isRising = true;

    void Start()
    {
        startPos  = new Vector3(transform.position.x, startOffsetY,  transform.position.z);
        targetPos = new Vector3(transform.position.x, targetOffsetY, transform.position.z);
        transform.position = startPos;

        Invoke(nameof(DestroySelf), selfDestroyTime);
    }

    void Update()
    {
        if (isRising)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPos, riseSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isRising = false;
            }
        }

        // bullet ถูกทำลายไปแล้ว → ทำลายโล่ด้วย
        if (targetBullet == null)
            DestroySelf();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log("[Shield] รับกระสุน — ทำลายทั้งคู่!");
            Destroy(other.gameObject);
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        CancelInvoke();

        // ✅ เช็คก่อน invoke
        try
        {
            onDestroyed?.Invoke();
        }
        catch { }

        Destroy(gameObject);
    }
}