using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRandomizer : MonoBehaviour
{
    [Header("ใส่ชื่อ Scene ของแต่ละด่าน (ต้อง Add ใน Build Settings ด้วย)")]
    public string[] levelSceneNames;

    [Header("ป้องกันสุ่มด่านซ้ำติดกัน (optional)")]
    public bool avoidRepeat = true;

    private int lastLevelIndex = -1;

    // เรียกฟังก์ชันนี้จากปุ่ม OnClick()
    public void RandomizeAndLoadLevel()
    {
        if (levelSceneNames == null || levelSceneNames.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้ใส่รายชื่อด่านใน levelSceneNames");
            return;
        }

        int index = GetRandomLevelIndex();
        lastLevelIndex = index;

        string sceneName = levelSceneNames[index];
        Debug.Log($"สุ่มได้ด่าน: {sceneName} (index {index})");

        SceneManager.LoadScene(sceneName);
    }

    private int GetRandomLevelIndex()
    {
        if (levelSceneNames.Length == 1) return 0;

        int index;
        do
        {
            index = Random.Range(0, levelSceneNames.Length);
        }
        while (avoidRepeat && index == lastLevelIndex);

        return index;
    }
}