using UnityEngine;

public class RandomFloorGenerator : MonoBehaviour
{
    [Header("Floor Prefabs")]
    [Tooltip("ใส่โมเดลลายพื้นต่างๆ (Prefabs) ที่ต้องการให้สุ่มลงในนี้")]
    public GameObject[] floorPrefabs;

    [Header("Grid Settings")]
    [Tooltip("จำนวนแผ่นพื้นในแนวแกน X")]
    public int width = 10;
    [Tooltip("จำนวนแผ่นพื้นในแนวแกน Z")]
    public int length = 10;
    [Tooltip("ขนาดความกว้าง/ยาวของโมเดลพื้น 1 แผ่น (เช่น ถ้าโมเดลขนาด 2x2 เมตร ให้ใส่ 2)")]
    public float tileSize = 2.0f;

    [Header("Randomization Settings")]
    [Tooltip("ถ้าติ๊กถูก จะทำการสุ่มหมุนโมเดลทีละ 90 องศา เพื่อช่วยลดการซ้ำของลาย")]
    public bool randomRotation = true;

    // ฟังก์ชันนี้จะคลิกขวาเรียกใช้จาก Inspector ได้เลย
    [ContextMenu("Generate Floor")]
    public void GenerateFloor()
    {
        // เคลียร์โมเดลเดิมออกก่อนเพื่อไม่ให้ทับกัน
        ClearFloor();

        if (floorPrefabs == null || floorPrefabs.Length == 0)
        {
            Debug.LogWarning("กรุณาใส่โมเดลพื้นในลิสต์ Floor Prefabs ก่อนรันฟังก์ชัน!");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                // 1. สุ่มเลือกโมเดลจากลิสต์ที่เราใส่ไว้
                int randomIndex = Random.Range(0, floorPrefabs.Length);
                GameObject selectedPrefab = floorPrefabs[randomIndex];

                if (selectedPrefab == null) continue;

                // 2. คำนวณตำแหน่งบน Grid อ้างอิงจากตำแหน่งของ Object นี้
                Vector3 spawnPosition = transform.position + new Vector3(x * tileSize, 0, z * tileSize);

                // 3. สุ่มหมุนวัตถุ (0, 90, 180, 270 องศา) เพื่อให้ลวดลายสลับทิศทาง
                Quaternion spawnRotation = Quaternion.identity;
                if (randomRotation)
                {
                    float[] rotationAngles = { 0f, 90f, 180f, 270f };
                    float randomYRotation = rotationAngles[Random.Range(0, rotationAngles.Length)];
                    spawnRotation = Quaternion.Euler(0, randomYRotation, 0);
                }

                // 4. สร้าง (Instantiate) วัตถุขึ้นมา
                GameObject newTile = Instantiate(selectedPrefab, spawnPosition, spawnRotation, transform);
                newTile.name = $"FloorTile_{x}_{z}";
            }
        }
        
        Debug.Log("สร้างพื้นสุ่มสำเร็จเรียบร้อย!");
    }

    [ContextMenu("Clear Floor")]
    public void ClearFloor()
    {
        // ลบ GameObject ลูกๆ ทั้งหมดที่ถูกสร้างขึ้น
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Debug.Log("เคลียร์พื้นเก่าเรียบร้อย!");
    }
}