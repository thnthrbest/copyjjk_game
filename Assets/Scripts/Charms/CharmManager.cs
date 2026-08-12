using System;
using System.Collections.Generic;
using UnityEngine;

namespace Charms
{
    public class CharmManager : MonoBehaviour
    {
        public static CharmManager Instance { get; private set; }

        [Header("Notch Settings")]
        public int maxNotchSlots = 4;

        [Header("Database (Optional ScriptableObject)")]
        public CharmDatabaseSO charmDatabase;

        [Header("Audio SFX")]
        public AudioClip equipSound;
        public AudioClip unequipSound;
        public AudioClip failSound;

        // PlayerPrefs Key
        private const string PrefsEquippedCharmsKey = "EquippedCharms";

        // Runtime Lists
        private List<CharmItem> allCharms = new List<CharmItem>();
        private HashSet<string> equippedCharmIds = new HashSet<string>();

        // Events for UI update
        public event Action OnCharmsChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeCharmsList();
                LoadEquippedCharms();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// เตรียมรายชื่อเครื่องรางทั้งหมด 7 ชิ้น (ดึงจาก SO หรือใช้อัตโนมัติถ้าไม่มี SO)
        /// </summary>
        private void InitializeCharmsList()
        {
            if (charmDatabase != null && charmDatabase.charms.Count > 0)
            {
                allCharms = new List<CharmItem>(charmDatabase.charms);
                return;
            }

            // Default fallback List ทั้งหมด 7 ชิ้นตามข้อกำหนด
            allCharms = new List<CharmItem>
            {
                new CharmItem
                {
                    id = "charm_fireball_heal",
                    charmName = "เครื่องรางเพลิงเยียวยา",
                    description = "เก็บลูกไฟได้เลือดเยอะขึ้น 50%",
                    notchCost = 2,
                    effectType = CharmEffectType.FireballHealBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_rabbit_duration",
                    charmName = "เครื่องรางกระต่ายท่องนภา",
                    description = "เพิ่มระยะเวลาของอาคมกระต่ายนานขึ้น 10 วินาที",
                    notchCost = 1,
                    effectType = CharmEffectType.RabbitDurationAdd,
                    value = 10f
                },
                new CharmItem
                {
                    id = "charm_wolf_summon",
                    charmName = "เครื่องรางจ่าฝูงหมาป่า",
                    description = "เพิ่มจำนวนการอัญเชิญหมาป่าของอาคมหมาป่า 1 ตัว",
                    notchCost = 1,
                    effectType = CharmEffectType.WolfSummonAdd,
                    value = 1f
                },
                new CharmItem
                {
                    id = "charm_deer_regen",
                    charmName = "เครื่องรางกวางพฤกษา",
                    description = "เพิ่มอัตราการฟื้นฟูของอาคมกวาง 50%",
                    notchCost = 1,
                    effectType = CharmEffectType.DeerRegenBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_bull_duration",
                    charmName = "เครื่องรางวัวถึกทรหด",
                    description = "เพิ่มระยะเวลาของอาคมวัวนานขึ้น 5 วินาที",
                    notchCost = 1,
                    effectType = CharmEffectType.BullDurationAdd,
                    value = 5f
                },
                new CharmItem
                {
                    id = "charm_base_attack",
                    charmName = "เครื่องรางทรงพลัง",
                    description = "เพิ่มพลังการโจมตีพื้นฐาน 50%",
                    notchCost = 2,
                    effectType = CharmEffectType.BaseAttackBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_attack_speed",
                    charmName = "เครื่องรางวายุว่องไว",
                    description = "เพิ่มความเร็วในการโจมตี 50%",
                    notchCost = 2,
                    effectType = CharmEffectType.AttackSpeedBoost,
                    value = 0.50f
                }
            };
        }

        // ─────────────────────────────
        //  Notch Logic & Validation
        // ─────────────────────────────

        public int GetCurrentUsedNotches()
        {
            int used = 0;
            foreach (var charm in allCharms)
            {
                if (equippedCharmIds.Contains(charm.id))
                {
                    used += charm.notchCost;
                }
            }
            return used;
        }

        public int GetRemainingNotches()
        {
            return maxNotchSlots - GetCurrentUsedNotches();
        }

        public bool CanEquip(CharmItem charm)
        {
            if (charm == null) return false;
            if (IsEquipped(charm.id)) return false;
            return (GetCurrentUsedNotches() + charm.notchCost) <= maxNotchSlots;
        }

        public bool IsEquipped(string charmId)
        {
            return equippedCharmIds.Contains(charmId);
        }

        // ─────────────────────────────
        //  Equip / Unequip Actions
        // ─────────────────────────────

        public bool EquipCharm(string charmId)
        {
            CharmItem charm = GetCharmById(charmId);
            if (charm == null)
            {
                Debug.LogWarning($"[CharmManager] ไม่พบเครื่องราง ID: {charmId}");
                PlaySFX(failSound);
                return false;
            }

            if (IsEquipped(charmId))
            {
                Debug.LogWarning($"[CharmManager] เครื่องราง {charm.charmName} สวมใส่อยู่แล้ว");
                return false;
            }

            if (!CanEquip(charm))
            {
                Debug.LogWarning($"[CharmManager] ช่องเครื่องรางไม่เพียงพอ! ต้องการ {charm.notchCost} ช่อง แต่เหลือเพียง {GetRemainingNotches()} ช่อง");
                PlaySFX(failSound);
                return false;
            }

            equippedCharmIds.Add(charmId);
            SaveEquippedCharms();
            PlaySFX(equipSound);
            Debug.Log($"[CharmManager] สวมใส่เครื่องราง: {charm.charmName} (ใช้ {charm.notchCost} ช่อง | รวมใช้ไป {GetCurrentUsedNotches()}/{maxNotchSlots})");

            OnCharmsChanged?.Invoke();
            return true;
        }

        public bool UnequipCharm(string charmId)
        {
            if (!IsEquipped(charmId))
            {
                Debug.LogWarning($"[CharmManager] เครื่องราง ID {charmId} ไม่ได้สวมใส่อยู่");
                return false;
            }

            equippedCharmIds.Remove(charmId);
            SaveEquippedCharms();
            PlaySFX(unequipSound);
            Debug.Log($"[CharmManager] ถอดเครื่องราง ID: {charmId} ออกเรียบร้อย");

            OnCharmsChanged?.Invoke();
            return true;
        }

        public void UnequipAll()
        {
            equippedCharmIds.Clear();
            SaveEquippedCharms();
            OnCharmsChanged?.Invoke();
        }

        // ─────────────────────────────
        //  Getters
        // ─────────────────────────────

        public List<CharmItem> GetAllCharms()
        {
            return allCharms;
        }

        public List<CharmItem> GetEquippedCharms()
        {
            List<CharmItem> list = new List<CharmItem>();
            foreach (var charm in allCharms)
            {
                if (equippedCharmIds.Contains(charm.id))
                {
                    list.Add(charm);
                }
            }
            return list;
        }

        public CharmItem GetCharmById(string id)
        {
            return allCharms.Find(c => c.id == id);
        }

        // ─────────────────────────────
        //  Gameplay Bonus Multiplier Helpers
        // ─────────────────────────────

        /// <summary>
        /// 1. เพิ่มการฮีลจากลูกไฟ 50% (คูณ 1.5)
        /// </summary>
        public float GetFireballHealMultiplier()
        {
            return IsEquipped("charm_fireball_heal") ? 1.5f : 1.0f;
        }

        /// <summary>
        /// 2. เพิ่มระยะเวลาอาคมกระต่าย 10 วิ
        /// </summary>
        public float GetRabbitExtraDuration()
        {
            return IsEquipped("charm_rabbit_duration") ? 10.0f : 0.0f;
        }

        /// <summary>
        /// 3. เพิ่มจำนวนหมาป่า 1 ตัว
        /// </summary>
        public int GetWolfExtraSummonCount()
        {
            return IsEquipped("charm_wolf_summon") ? 1 : 0;
        }

        /// <summary>
        /// 4. เพิ่มอัตราการฟื้นฟูอาคมกวาง 50% (คูณ 1.5)
        /// </summary>
        public float GetDeerRegenMultiplier()
        {
            return IsEquipped("charm_deer_regen") ? 1.5f : 1.0f;
        }

        /// <summary>
        /// 5. เพิ่มระยะเวลาอาคมวัว 5 วิ
        /// </summary>
        public float GetBullExtraDuration()
        {
            return IsEquipped("charm_bull_duration") ? 5.0f : 0.0f;
        }

        /// <summary>
        /// 6. เพิ่มพลังโจมตีพื้นฐาน 50% (คูณ 1.5)
        /// </summary>
        public float GetBaseAttackMultiplier()
        {
            return IsEquipped("charm_base_attack") ? 1.5f : 1.0f;
        }

        /// <summary>
        /// 7. เพิ่มความเร็วในการโจมตี 50% (คูณ 1.5)
        /// </summary>
        public float GetAttackSpeedMultiplier()
        {
            return IsEquipped("charm_attack_speed") ? 1.5f : 1.0f;
        }

        // ─────────────────────────────
        //  Save / Load
        // ─────────────────────────────

        private void SaveEquippedCharms()
        {
            string[] array = new string[equippedCharmIds.Count];
            equippedCharmIds.CopyTo(array);
            string json = JsonUtility.ToJson(new CharmSaveData { equippedIds = array });
            PlayerPrefs.SetString(PrefsEquippedCharmsKey, json);
            PlayerPrefs.Save();
        }

        private void LoadEquippedCharms()
        {
            equippedCharmIds.Clear();
            if (PlayerPrefs.HasKey(PrefsEquippedCharmsKey))
            {
                string json = PlayerPrefs.GetString(PrefsEquippedCharmsKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        CharmSaveData saveData = JsonUtility.FromJson<CharmSaveData>(json);
                        if (saveData != null && saveData.equippedIds != null)
                        {
                            foreach (string id in saveData.equippedIds)
                            {
                                equippedCharmIds.Add(id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CharmManager] Error loading charms save data: {ex.Message}");
                    }
                }
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(clip);
            }
        }

        [Serializable]
        private class CharmSaveData
        {
            public string[] equippedIds;
        }
    }
}
