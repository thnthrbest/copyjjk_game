using UnityEngine;

/// <summary>
/// (ตัวเสริม) ระบบ Distance Culling สำหรับวัตถุประดับที่ไม่ได้อยู่ในระบบโซนของ StagePathController
/// เช่น ต้นไม้กระจัดกระจาย, ไฟประดับ, VFX ที่วางแบบ Freeform ทั่วฉาก
///
/// ⚠️ สำหรับศัตรู (โยไค) และฉากในแต่ละด่าน ให้ใช้ระบบ Zone Culling ใน StagePathController แทน
///    โดยจัดวัตถุเข้า enemyZone / sceneryZone ของแต่ละ StagePoint
///
/// วิธีใช้: แนบสคริปต์นี้กับวัตถุประดับที่ต้องการ Cull เป็นราย ๆ ตัว
/// </summary>
public class DistanceCuller : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("ระยะห่างสูงสุดจากผู้เล่นก่อนปิดตัววัตถุ (หน่วย Unity)")]
    public float cullDistance = 45f;

    [Tooltip("ระยะที่ใกล้พอจะเปิดตัววัตถุกลับมา (ควรน้อยกว่า cullDistance เพื่อป้องกัน flickering)")]
    public float activateDistance = 40f;

    [Tooltip("ตรวจสอบระยะทุกกี่เฟรม (1 = ทุกเฟรม, 5 = ทุก 5 เฟรม) เพื่อลดภาระ CPU")]
    [Range(1, 30)]
    public int checkEveryNFrames = 5;

    // Internal
    private Transform playerTransform;
    private bool isCulled = false;
    private int frameOffset;
    private Renderer[] renderers;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"[DistanceCuller] ไม่พบ Player (Tag = Player) บน {gameObject.name}");
            enabled = false;
            return;
        }

        // ใส่ offset แบบสุ่มเพื่อกระจายภาระ
        frameOffset = Random.Range(0, checkEveryNFrames);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        if (playerTransform == null) return;
        if ((Time.frameCount + frameOffset) % checkEveryNFrames != 0) return;

        float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;

        if (!isCulled && sqrDist > cullDistance * cullDistance)
        {
            SetCulled(true);
        }
        else if (isCulled && sqrDist < activateDistance * activateDistance)
        {
            SetCulled(false);
        }
    }

    void SetCulled(bool culled)
    {
        isCulled = culled;
        if (renderers != null)
        {
            foreach (var r in renderers)
                r.enabled = !culled;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, cullDistance);
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, activateDistance);
    }
}
