using UnityEngine;

public class SkillManager : MonoBehaviour
{
      [Header("Unlock Status")]
      public bool isRabbitUnlocked = false;
      public bool isDogUnlocked = false;
      public bool isCowUnlocked = false;

      [Header("Persistence")]
      public bool saveToPlayerPrefs = true;

      void Start()
      {
          LoadStates();
      }

      public bool IsSkillUnlocked(string skillName)
      {
          switch (skillName.ToLower())
          {
              case "rabbit": return isRabbitUnlocked;
              case "dog": return isDogUnlocked;
              case "cow": return isCowUnlocked;
              default: return false;
          }
      }

      public void UnlockSkill(string skillName)
      {
          SetSkillState(skillName, true);
          Debug.Log($"[SkillManager] สกิล '{skillName}' ถูกปลดล็อคแล้ว!");
      }

      public void LockSkill(string skillName)
      {
          SetSkillState(skillName, false);
          Debug.Log($"[SkillManager] สกิล '{skillName}' ถูกล็อค!");
      }

      private void SetSkillState(string skillName, bool state)
      {
          switch (skillName.ToLower())
          {
              case "rabbit":
                  isRabbitUnlocked = state;
                  if (saveToPlayerPrefs) PlayerPrefs.SetInt("Skill_Rabbit", state ? 1 : 0);
                  break;
              case "dog":
                  isDogUnlocked = state;
                  if (saveToPlayerPrefs) PlayerPrefs.SetInt("Skill_Dog", state ? 1 : 0);
                  break;
              case "cow":
                  isCowUnlocked = state;
                  if (saveToPlayerPrefs) PlayerPrefs.SetInt("Skill_Cow", state ? 1 : 0);
                  break;
          }
          if (saveToPlayerPrefs) PlayerPrefs.Save();
      }

      private void LoadStates()
      {
          if (saveToPlayerPrefs)
          {
              isRabbitUnlocked = PlayerPrefs.GetInt("Skill_Rabbit", isRabbitUnlocked ? 1 : 0) == 1;
              isDogUnlocked = PlayerPrefs.GetInt("Skill_Dog", isDogUnlocked ? 1 : 0) == 1;
              isCowUnlocked = PlayerPrefs.GetInt("Skill_Cow", isCowUnlocked ? 1 : 0) == 1;
          }
      }

      // สำหรับการโกง/ทดสอบด่วนใน Editor
      [ContextMenu("Reset All PlayerPrefs")]
      public void ResetPlayerPrefs()
      {
          PlayerPrefs.DeleteKey("Skill_Rabbit");
          PlayerPrefs.DeleteKey("Skill_Dog");
          PlayerPrefs.DeleteKey("Skill_Cow");
          PlayerPrefs.Save();
          isRabbitUnlocked = false;
          isDogUnlocked = false;
          isCowUnlocked = false;
          Debug.Log("[SkillManager] รีเซ็ตการปลดล็อคสกิลทั้งหมดแล้ว!");
      }
}
