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
        public AudioClip boxOpenSound;

        // PlayerPrefs Keys
        private const string PrefsOwnedCharmsKey = "OwnedCharms_Data";
        private const string PrefsEquippedCharmsKey = "EquippedCharms_List";

        // Runtime State
        private List<CharmItem> allCharms = new List<CharmItem>();
        private Dictionary<string, int> ownedCharmsCount = new Dictionary<string, int>();
        private List<string> equippedCharmIdsList = new List<string>();

        // Current Run State (Box drops collected in active stage run)
        private int currentRunBoxesCount = 0;

        // Events for UI Updates
        public event Action OnCharmsChanged;
        public event Action<int> OnBoxesCountChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeCharmsList();
                LoadData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// เตรียมรายชื่อเครื่องรางทั้งหมด 7 ชิ้น
        /// </summary>
        private void InitializeCharmsList()
        {
            if (charmDatabase != null && charmDatabase.charms.Count > 0)
            {
                allCharms = new List<CharmItem>(charmDatabase.charms);
                return;
            }

            allCharms = new List<CharmItem>
            {
                new CharmItem
                {
                    id = "charm_fireball_heal",
                    charmName = "เครื่องรางเพลิงเยียวยา",
                    description = "เก็บลูกไฟได้เลือดเยอะขึ้น 50% ต่อชิ้น",
                    notchCost = 2,
                    effectType = CharmEffectType.FireballHealBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_rabbit_duration",
                    charmName = "เครื่องรางกระต่ายท่องนภา",
                    description = "เพิ่มระยะเวลาของอาคมกระต่ายนานขึ้น 10 วินาทีต่อชิ้น",
                    notchCost = 1,
                    effectType = CharmEffectType.RabbitDurationAdd,
                    value = 10f
                },
                new CharmItem
                {
                    id = "charm_wolf_summon",
                    charmName = "เครื่องรางจ่าฝูงหมาป่า",
                    description = "เพิ่มจำนวนการอัญเชิญหมาป่าของอาคมหมาป่า 1 ตัวต่อชิ้น",
                    notchCost = 1,
                    effectType = CharmEffectType.WolfSummonAdd,
                    value = 1f
                },
                new CharmItem
                {
                    id = "charm_deer_regen",
                    charmName = "เครื่องรางกวางพฤกษา",
                    description = "เพิ่มอัตราการฟื้นฟูของอาคมกวาง 50% ต่อชิ้น",
                    notchCost = 1,
                    effectType = CharmEffectType.DeerRegenBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_bull_duration",
                    charmName = "เครื่องรางวัวถึกทรหด",
                    description = "เพิ่มระยะเวลาของอาคมวัวนานขึ้น 5 วินาทีต่อชิ้น",
                    notchCost = 1,
                    effectType = CharmEffectType.BullDurationAdd,
                    value = 5f
                },
                new CharmItem
                {
                    id = "charm_base_attack",
                    charmName = "เครื่องรางทรงพลัง",
                    description = "เพิ่มพลังการโจมตีพื้นฐาน 50% ต่อชิ้น",
                    notchCost = 2,
                    effectType = CharmEffectType.BaseAttackBoost,
                    value = 0.50f
                },
                new CharmItem
                {
                    id = "charm_attack_speed",
                    charmName = "เครื่องรางวายุว่องไว",
                    description = "เพิ่มความเร็วในการโจมตี 50% ต่อชิ้น",
                    notchCost = 2,
                    effectType = CharmEffectType.AttackSpeedBoost,
                    value = 0.50f
                }
            };
        }

        // ─────────────────────────────
        //  Box Drop & Gacha System
        // ─────────────────────────────

        public void AddRunBox(int amount = 1)
        {
            currentRunBoxesCount += amount;
            Debug.Log($"[CharmManager] 📦 ได้รับกล่องเครื่องราง! ในรอบนี้รวมสะสมได้: {currentRunBoxesCount} กล่อง");
            OnBoxesCountChanged?.Invoke(currentRunBoxesCount);
        }

        public int GetRunBoxesCount()
        {
            return currentRunBoxesCount;
        }

        public void ResetRunBoxes()
        {
            currentRunBoxesCount = 0;
            OnBoxesCountChanged?.Invoke(currentRunBoxesCount);
        }

        /// <summary>
        /// เปิดกล่องเครื่องราง 1 กล่อง สุ่มได้ 1 ชิ้น (สุ่มออกซ้ำได้)
        /// </summary>
        public CharmItem OpenOneBox()
        {
            if (allCharms.Count == 0) return null;

            // สุ่ม 1 ชิ้นจากที่มีทั้งหมด
            int randomIndex = UnityEngine.Random.Range(0, allCharms.Count);
            CharmItem pulledCharm = allCharms[randomIndex];

            // เพิ่มจำนวนในคลังสะสม
            AddOwnedCharm(pulledCharm.id, 1);

            if (currentRunBoxesCount > 0)
            {
                currentRunBoxesCount--;
                OnBoxesCountChanged?.Invoke(currentRunBoxesCount);
            }

            PlaySFX(boxOpenSound);
            Debug.Log($"[CharmManager] 🎉 เปิดกล่องได้: {pulledCharm.charmName}! (จำนวนรวมในครอบครอง: {GetOwnedCount(pulledCharm.id)} ชิ้น)");
            return pulledCharm;
        }

        // ─────────────────────────────
        //  Owned & Stackable Inventory
        // ─────────────────────────────

        public int GetOwnedCount(string charmId)
        {
            if (ownedCharmsCount.TryGetValue(charmId, out int count))
            {
                return count;
            }
            return 0;
        }

        public void AddOwnedCharm(string charmId, int amount = 1)
        {
            if (!ownedCharmsCount.ContainsKey(charmId))
            {
                ownedCharmsCount[charmId] = 0;
            }
            ownedCharmsCount[charmId] += amount;
            SaveData();
            OnCharmsChanged?.Invoke();
        }

        public int GetEquippedCount(string charmId)
        {
            int count = 0;
            foreach (string id in equippedCharmIdsList)
            {
                if (id == charmId) count++;
            }
            return count;
        }

        // ─────────────────────────────
        //  Notch & Equip Logic (Stackable)
        // ─────────────────────────────

        public int GetCurrentUsedNotches()
        {
            int used = 0;
            foreach (string id in equippedCharmIdsList)
            {
                CharmItem charm = GetCharmById(id);
                if (charm != null)
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

            // เช็คว่ามีของในคลังพอให้ใส่อีกชิ้นไหม
            if (GetEquippedCount(charm.id) >= GetOwnedCount(charm.id))
            {
                return false;
            }

            // เช็คว่า Notch เหลือพอไหม
            return (GetCurrentUsedNotches() + charm.notchCost) <= maxNotchSlots;
        }

        public bool EquipCharm(string charmId)
        {
            CharmItem charm = GetCharmById(charmId);
            if (charm == null)
            {
                Debug.LogWarning($"[CharmManager] ไม่พบเครื่องราง ID: {charmId}");
                PlaySFX(failSound);
                return false;
            }

            if (!CanEquip(charm))
            {
                if (GetEquippedCount(charmId) >= GetOwnedCount(charmId))
                {
                    Debug.LogWarning($"[CharmManager] จำนวนเครื่องราง {charm.charmName} ในคลังถูกสวมใส่ครบแล้ว ({GetEquippedCount(charmId)}/{GetOwnedCount(charmId)})");
                }
                else
                {
                    Debug.LogWarning($"[CharmManager] ช่องเครื่องรางไม่เพียงพอ! ต้องการ {charm.notchCost} ช่อง แต่เหลือเพียง {GetRemainingNotches()} ช่อง");
                }
                PlaySFX(failSound);
                return false;
            }

            equippedCharmIdsList.Add(charmId);
            SaveData();
            PlaySFX(equipSound);
            Debug.Log($"[CharmManager] สวมใส่เครื่องราง: {charm.charmName} (ใช้ไป {GetCurrentUsedNotches()}/{maxNotchSlots} ช่อง)");

            OnCharmsChanged?.Invoke();
            return true;
        }

        public bool UnequipCharm(string charmId)
        {
            if (!equippedCharmIdsList.Contains(charmId))
            {
                Debug.LogWarning($"[CharmManager] เครื่องราง ID {charmId}ไม่ได้สวมใส่อยู่");
                return false;
            }

            equippedCharmIdsList.Remove(charmId);
            SaveData();
            PlaySFX(unequipSound);
            Debug.Log($"[CharmManager] ถอดเครื่องราง ID: {charmId} ออก 1 ชิ้น");

            OnCharmsChanged?.Invoke();
            return true;
        }

        public void UnequipAll()
        {
            equippedCharmIdsList.Clear();
            SaveData();
            OnCharmsChanged?.Invoke();
        }

        // ─────────────────────────────
        //  Getters
        // ─────────────────────────────

        public List<CharmItem> GetAllCharms()
        {
            return allCharms;
        }

        public List<string> GetEquippedCharmIdsList()
        {
            return new List<string>(equippedCharmIdsList);
        }

        public CharmItem GetCharmById(string id)
        {
            return allCharms.Find(c => c.id == id);
        }

        // ─────────────────────────────
        //  Stacking Gameplay Multiplier Helpers
        // ─────────────────────────────

        /// <summary>
        /// 1. เพิ่มการฮีลจากลูกไฟ 50% ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetFireballHealMultiplier()
        {
            int count = GetEquippedCount("charm_fireball_heal");
            return 1.0f + (count * 0.50f);
        }

        /// <summary>
        /// 2. เพิ่มระยะเวลาอาคมกระต่าย +10 วิ ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetRabbitExtraDuration()
        {
            int count = GetEquippedCount("charm_rabbit_duration");
            return count * 10.0f;
        }

        /// <summary>
        /// 3. เพิ่มจำนวนหมาป่า +1 ตัว ต่อชิ้นที่สวมใส่
        /// </summary>
        public int GetWolfExtraSummonCount()
        {
            return GetEquippedCount("charm_wolf_summon");
        }

        /// <summary>
        /// 4. เพิ่มอัตราการฟื้นฟูอาคมกวาง 50% ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetDeerRegenMultiplier()
        {
            int count = GetEquippedCount("charm_deer_regen");
            return 1.0f + (count * 0.50f);
        }

        /// <summary>
        /// 5. เพิ่มระยะเวลาอาคมวัว +5 วิ ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetBullExtraDuration()
        {
            int count = GetEquippedCount("charm_bull_duration");
            return count * 5.0f;
        }

        /// <summary>
        /// 6. เพิ่มพลังโจมตีพื้นฐาน 50% ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetBaseAttackMultiplier()
        {
            int count = GetEquippedCount("charm_base_attack");
            return 1.0f + (count * 0.50f);
        }

        /// <summary>
        /// 7. เพิ่มความเร็วในการโจมตี 50% ต่อชิ้นที่สวมใส่
        /// </summary>
        public float GetAttackSpeedMultiplier()
        {
            int count = GetEquippedCount("charm_attack_speed");
            return 1.0f + (count * 0.50f);
        }

        // ─────────────────────────────
        //  Save / Load Data (JSON in PlayerPrefs)
        // ─────────────────────────────

        private void SaveData()
        {
            // 1. Save Owned
            List<OwnedEntry> ownedList = new List<OwnedEntry>();
            foreach (var kvp in ownedCharmsCount)
            {
                ownedList.Add(new OwnedEntry { id = kvp.Key, count = kvp.Value });
            }
            string jsonOwned = JsonUtility.ToJson(new OwnedContainer { entries = ownedList.ToArray() });
            PlayerPrefs.SetString(PrefsOwnedCharmsKey, jsonOwned);

            // 2. Save Equipped List
            string jsonEquipped = JsonUtility.ToJson(new EquippedContainer { list = equippedCharmIdsList.ToArray() });
            PlayerPrefs.SetString(PrefsEquippedCharmsKey, jsonEquipped);

            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            ownedCharmsCount.Clear();
            equippedCharmIdsList.Clear();

            // Load Owned Data
            if (PlayerPrefs.HasKey(PrefsOwnedCharmsKey))
            {
                string jsonOwned = PlayerPrefs.GetString(PrefsOwnedCharmsKey, "");
                if (!string.IsNullOrEmpty(jsonOwned))
                {
                    try
                    {
                        OwnedContainer container = JsonUtility.FromJson<OwnedContainer>(jsonOwned);
                        if (container != null && container.entries != null)
                        {
                            foreach (var entry in container.entries)
                            {
                                ownedCharmsCount[entry.id] = entry.count;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CharmManager] Error loading owned charms: {ex.Message}");
                    }
                }
            }

            // Load Equipped List Data
            if (PlayerPrefs.HasKey(PrefsEquippedCharmsKey))
            {
                string jsonEquipped = PlayerPrefs.GetString(PrefsEquippedCharmsKey, "");
                if (!string.IsNullOrEmpty(jsonEquipped))
                {
                    try
                    {
                        EquippedContainer container = JsonUtility.FromJson<EquippedContainer>(jsonEquipped);
                        if (container != null && container.list != null)
                        {
                            foreach (string id in container.list)
                            {
                                equippedCharmIdsList.Add(id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CharmManager] Error loading equipped charms: {ex.Message}");
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
        private class OwnedEntry
        {
            public string id;
            public int count;
        }

        [Serializable]
        private class OwnedContainer
        {
            public OwnedEntry[] entries;
        }

        [Serializable]
        private class EquippedContainer
        {
            public string[] list;
        }
    }
}
