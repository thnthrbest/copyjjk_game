using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Charms
{
    public class CharmsUIController : MonoBehaviour
    {
        [Header("Notch Info UI")]
        public TextMeshProUGUI notchStatusText; // e.g. "ช่องเครื่องรางที่ใช้: 3 / 4"
        public Image[] notchSlots;               // Visual slot icons (e.g. 4 slot images)
        public Color activeSlotColor = Color.yellow;
        public Color emptySlotColor = Color.gray;

        [Header("Simple Default UI (if using direct inspector button list)")]
        public Button[] defaultCharmButtons;    // Optional manual button list for 7 charms

        private void Start()
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.OnCharmsChanged += RefreshUI;
            }

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.OnCharmsChanged -= RefreshUI;
            }
        }

        public void RefreshUI()
        {
            if (CharmManager.Instance == null) return;

            int used = CharmManager.Instance.GetCurrentUsedNotches();
            int max = CharmManager.Instance.maxNotchSlots;

            // 1. Update notch status text
            if (notchStatusText != null)
            {
                notchStatusText.text = $"ช่องเครื่องรางที่ใช้: {used} / {max}";
            }

            // 2. Update visual slots
            if (notchSlots != null)
            {
                for (int i = 0; i < notchSlots.Length; i++)
                {
                    if (notchSlots[i] != null)
                    {
                        notchSlots[i].color = (i < used) ? activeSlotColor : emptySlotColor;
                    }
                }
            }

            // 3. Update charm buttons if assigned directly in Inspector
            List<CharmItem> allCharms = CharmManager.Instance.GetAllCharms();
            if (defaultCharmButtons != null && defaultCharmButtons.Length > 0)
            {
                for (int i = 0; i < defaultCharmButtons.Length && i < allCharms.Count; i++)
                {
                    Button btn = defaultCharmButtons[i];
                    if (btn == null) continue;

                    CharmItem item = allCharms[i];
                    int owned = CharmManager.Instance.GetOwnedCount(item.id);
                    int equipped = CharmManager.Instance.GetEquippedCount(item.id);
                    bool canEquip = CharmManager.Instance.CanEquip(item);

                    // Update button state / text
                    TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        if (owned == 0)
                        {
                            btnText.text = $"{item.charmName}\n(ยังไม่มีในคลัง)";
                        }
                        else
                        {
                            btnText.text = $"{item.charmName} ({item.notchCost} ช่อง)\nใส่แล้ว {equipped} / มี {owned} ชิ้น";
                        }
                    }

                    // Enable button if player owns item AND (can equip another OR has at least 1 equipped to unequip)
                    btn.interactable = (owned > 0) && (canEquip || equipped > 0);

                    // Bind click: If max equipped or can't equip, click unequips; otherwise equips another
                    string charmId = item.id;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnCharmButtonClicked(charmId));
                }
            }
        }

        public void OnCharmButtonClicked(string charmId)
        {
            if (CharmManager.Instance == null) return;

            CharmItem item = CharmManager.Instance.GetCharmById(charmId);
            if (item == null) return;

            int owned = CharmManager.Instance.GetOwnedCount(charmId);
            int equipped = CharmManager.Instance.GetEquippedCount(charmId);

            // If we can equip another copy, equip it. Otherwise if already equipped, unequip one copy.
            if (CharmManager.Instance.CanEquip(item))
            {
                CharmManager.Instance.EquipCharm(charmId);
            }
            else if (equipped > 0)
            {
                CharmManager.Instance.UnequipCharm(charmId);
            }
        }

        public void EquipOne(string charmId)
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.EquipCharm(charmId);
            }
        }

        public void UnequipOne(string charmId)
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.UnequipCharm(charmId);
            }
        }
    }
}
