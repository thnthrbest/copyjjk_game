using UnityEngine;

/// <summary>
/// ระบบจัดสรรหน่วยความจำและซ่อน/แสดงห้อง (Room-based Culling)
/// สำหรับสวมใส่เข้ากับ GameObject ของแต่ละด่าน/โซน เพื่อสลับการทำงานของวัตถุตามตำแหน่งผู้เล่น
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomCuller : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("กลุ่มวัตถุของด่าน/ห้องนี้ที่จะใช้เปิดหรือปิดการแสดงผล")]
    public GameObject stageContent;

    [Tooltip("เปิดใช้งานห้องนี้ตั้งแต่เริ่มเกมหรือไม่ (สำหรับห้องจุดเกิดเริ่มต้น)")]
    public bool isActiveOnStart = false;

    private void Awake()
    {
        // ตรวจสอบความถูกต้องของ Collider
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[RoomCuller] Collider บน '{gameObject.name}' ไม่ได้ตั้งค่าเป็น Trigger ระบบได้ทำการปรับเป็น Trigger ให้แล้ว");
        }

        if (stageContent == null)
        {
            Debug.LogError($"[RoomCuller] กรุณาลาก Group วัตถุของห้องมาใส่ใน stageContent บน '{gameObject.name}'", this);
        }
    }

    private void Start()
    {
        // ตั้งสถานะการแสดงผลเริ่มต้นของห้อง
        if (stageContent != null)
        {
            stageContent.SetActive(isActiveOnStart);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // เมื่อวัตถุที่มี tag "Player" เดินเข้ามาใน Trigger ให้เปิดการทำงานของห้อง
        if (other.CompareTag("Player"))
        {
            if (stageContent != null)
            {
                stageContent.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // เมื่อวัตถุที่มี tag "Player" เดินออกนอก Trigger ให้ปิดการทำงานของห้องเพื่อประหยัดหน่วยความจำ
        if (other.CompareTag("Player"))
        {
            if (stageContent != null)
            {
                stageContent.SetActive(false);
            }
        }
    }
}
