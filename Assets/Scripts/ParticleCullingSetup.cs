using UnityEngine;

/// <summary>
/// สคริปต์สำหรับค้นหาและตั้งค่า Culling Mode ของ Particle System ทุกตัวในฉาก
/// ให้เป็น "Pause" หรือ "Pause and Catch-Up" เพื่อไม่ให้รัน Simulation
/// เวลาอนุภาคอยู่นอกระยะของกล้อง
///
/// วิธีใช้: แนบสคริปต์นี้กับ GameObject ใดก็ได้ในฉาก (เช่น GameManager)
/// จะทำงานครั้งเดียวตอน Start แล้วปิดตัวเองอัตโนมัติ
/// </summary>
public class ParticleCullingSetup : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("โหมด Culling ที่ต้องการตั้งให้ Particle System ทุกตัว")]
    public ParticleSystemCullingMode targetCullingMode = ParticleSystemCullingMode.PauseAndCatchup;

    [Tooltip("ตั้ง Max Particles สูงสุดต่อ Particle System (0 = ไม่เปลี่ยน)")]
    public int maxParticlesOverride = 0;

    void Start()
    {
        ApplyParticleCulling();

        // ปิดตัวเองหลังทำงานเสร็จ — ไม่ต้องเปลือง Update
        enabled = false;
    }

    /// <summary>
    /// ค้นหา Particle System ทุกตัวในฉากแล้วตั้ง Culling Mode
    /// </summary>
    [ContextMenu("Apply Particle Culling Settings")]
    public void ApplyParticleCulling()
    {
        // FindObjectsOfType(true) จะหาทั้ง active และ inactive GameObjects
        ParticleSystem[] allParticles = FindObjectsOfType<ParticleSystem>(true);

        int count = 0;
        foreach (ParticleSystem ps in allParticles)
        {
            // ตั้ง Culling Mode
            var main = ps.main;
            if (main.cullingMode != targetCullingMode)
            {
                main.cullingMode = targetCullingMode;
                count++;
            }

            // จำกัด Max Particles (ถ้าต้องการ)
            if (maxParticlesOverride > 0 && main.maxParticles > maxParticlesOverride)
            {
                main.maxParticles = maxParticlesOverride;
            }
        }

        Debug.Log($"[ParticleCullingSetup] ตั้ง Culling Mode = {targetCullingMode} " +
                  $"ให้ Particle System {count}/{allParticles.Length} ตัว");
    }
}
