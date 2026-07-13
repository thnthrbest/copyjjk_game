using UnityEngine;

public class LaserGridController : MonoBehaviour
{
    [Header("Laser Mesh Settings")]
    [SerializeField] private GameObject cylinderLaserPrefab; // พรีแฟบแท่งทรงกระบอก 3D
    [SerializeField] private int totalLasers = 3;            // แนะนำ 3 เส้นบีบรวมกันเพื่อให้เลเซอร์ดูหนาแน่นเป็นลำเดียว
    [SerializeField] private float gridWidth = 1.5f;          // ความกว้างแผง (ตั้งค่าน้อยๆ ลำแสงจะบีบรวมกันสวย)
    [SerializeField] private float maxLaserLength = 40f;      // ระยะยิงไกลสุดในฉาก 3D
    [SerializeField] private LayerMask collisionMask;        // เลเยอร์ของกำแพงและผู้เล่น

    private GameObject[] laserInstances;
    private Vector3[] localStartPositions; 
    private float initialCylinderHeight = 2f; // ค่าความสูงเริ่มต้นของ Cylinder มาตรฐานใน Unity
    private bool isFiring = false; 

    void Start()
    {
        laserInstances = new GameObject[totalLasers];
        localStartPositions = new Vector3[totalLasers]; 
        float spacing = totalLasers > 1 ? gridWidth / (totalLasers - 1) : 0;

        for (int i = 0; i < totalLasers; i++)
        {
            if (cylinderLaserPrefab != null)
            {
                // กระจายจุดเกิดเลเซอร์ย่อยในแนวระนาบ X ของปืน
                localStartPositions[i] = new Vector3(-gridWidth / 2 + (i * spacing), 0, 0);
                
                GameObject laserObj = Instantiate(cylinderLaserPrefab, transform);
                laserObj.transform.localPosition = localStartPositions[i];
                
                // สำหรับ 3D: หมุนวัตถุทรงกระบอกตัวลูกให้หันหน้าชี้ไปทางแกน Z (หมุนแกน X ขึ้นมา 90 องศา)
                // เพื่อให้เนื้อโมเดลพุ่งขนานไปกับแนวเส้น Raycast
                laserObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                
                laserInstances[i] = laserObj;
                laserObj.SetActive(false); // ปิดซ่อนไว้ก่อนตั้งแต่เริ่มเกม
            }
        }
    }

    void Update()
    {
        // ถ้ายังไม่ถึงเฟสยิง (บอสกำลังชาร์จ) ให้ข้ามการทำงานไปก่อนเพื่อไม่ให้เปลืองทรัพยากรเครื่อง
        if (!isFiring) return;

        // ยิง Raycast ออกไปตามทิศทางด้านหน้า (แกน Z สีน้ำเงิน) ในระบบ 3D
        Vector3 direction = transform.forward; 

        for (int i = 0; i < totalLasers; i++)
        {
            if (laserInstances[i] == null) continue;

            Transform laserTransform = laserInstances[i].transform;
            Vector3 worldStartPos = transform.TransformPoint(localStartPositions[i]);

            if (Physics.Raycast(worldStartPos, direction, out RaycastHit hit, maxLaserLength, collisionMask))
            {
                float currentLength = hit.distance;
                
                // ปรับสเกลความยาว (แกน Y ของตัวทรงกระบอก) ให้ยาวเท่ากับระยะทางที่ Raycast ชนกำแพงพอดี
                Vector3 newScale = laserTransform.localScale;
                newScale.y = currentLength / initialCylinderHeight;
                laserTransform.localScale = newScale;

                // เลื่อนตำแหน่งโมเดลให้อยู่กึ่งกลางระหว่างจุดปล่อยจนถึงจุดปะทะกำแพง (เนื่องจาก Pivot อยู่ตรงกลางก้อน)
                laserTransform.position = worldStartPos + direction * (currentLength / 2f);

                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("ผู้เล่นหนีวิถีเลเซอร์ไม่พ้น! โดนดาเมจเต็มๆ!");
                }
            }
            else
            {
                // ถ้ารอดกำแพง ยิงยาวสุดขอบฟ้าตามระยะ Max
                Vector3 newScale = laserTransform.localScale;
                newScale.y = maxLaserLength / initialCylinderHeight;
                laserTransform.localScale = newScale;

                laserTransform.position = worldStartPos + direction * (maxLaserLength / 2f);
            }
        }
    }

    // ฟังก์ชันเปิด/ปิด ลำแสงเลเซอร์ ที่เรียกสั่งการมาจากสคริปต์บอส
    public void ActivateFullLaser(bool state)
    {
        isFiring = state;
        foreach (GameObject laser in laserInstances)
        {
            if (laser != null) laser.SetActive(state);
        }
    }
}