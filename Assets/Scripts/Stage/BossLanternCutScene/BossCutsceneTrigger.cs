using UnityEngine;

public class BossCutsceneTrigger : MonoBehaviour
{
    public BossIntroCutscene cutscene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cutscene.Play();
            gameObject.SetActive(false); // กัน trigger ซ้ำ
        }
    }
}