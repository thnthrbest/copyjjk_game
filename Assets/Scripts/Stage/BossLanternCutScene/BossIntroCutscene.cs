using UnityEngine;
using System.Collections;

public class BossIntroCutscene : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera cutsceneCamera;

    [Header("Boss (Real)")]
    public GameObject realBoss;
    public MonoBehaviour bossAI;

    [Header("Boss (Fake)")]
    public GameObject bossFakePrefab;

    [Header("Points")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    [Header("Timing")]
    public float moveToBTime = 1.0f;
    public float moveToCTime = 0.8f;
    public float delayBeforeDestroy = 0.5f;

    private bool triggered = false;

    public void Play()
    {
        if (triggered) return;
        triggered = true;

        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        // 🔻 ซ่อน Player
        player.SetActive(false);

        // 🔻 ปิดบอสตัวจริง
        if (bossAI != null) bossAI.enabled = false;
        realBoss.SetActive(false);

        // 🎥 สลับกล้อง
        mainCamera.enabled = false;
        cutsceneCamera.enabled = true;

        // 👻 Spawn บอสหลอก
        GameObject fake = Instantiate(bossFakePrefab, pointA.position, Quaternion.identity);
        Animator anim = fake.GetComponent<Animator>();

        if (anim != null)
        {
            anim.Play("CutScene", 0, 0f);
        }

        // ⏫ เคลื่อน A → B (เหมือนกระโดดขึ้น)
        yield return MoveOverTime(fake.transform, pointA.position, pointB.position, moveToBTime);

        // ⏬ เคลื่อน B → C (ตกลงมา)
        yield return MoveOverTime(fake.transform, pointB.position, pointC.position, moveToCTime);

        // ⏳ รอเล็กน้อยให้ท่า animation จบฟีล
        yield return new WaitForSeconds(delayBeforeDestroy);

        // 💥 ลบบอสหลอก
        Destroy(fake);

        // 🎥 กลับกล้องหลัก
        cutsceneCamera.enabled = false;
        mainCamera.enabled = true;

        // 👿 เปิดบอสตัวจริง
        realBoss.transform.position = pointC.position;
        realBoss.SetActive(true);

        if (bossAI != null) bossAI.enabled = true;

        // 🔺 เอา Player กลับมา
        player.SetActive(true);
    }

    IEnumerator MoveOverTime(Transform obj, Vector3 start, Vector3 end, float time)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / time;

            // ease ให้ลื่น
            float smoothT = Mathf.SmoothStep(0, 1, t);

            obj.position = Vector3.Lerp(start, end, smoothT);

            yield return null;
        }
    }
}