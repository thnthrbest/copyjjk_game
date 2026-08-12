using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Charms
{
    public class CharmsUIController : MonoBehaviour
    {
        [Header("Notch Info UI")]
        public TextMeshProUGUI notchStatusText; // e.g. "Charms Equipped: 3 / 4 Notches"
        public Image[] notchSlots;               // Visual slot icons (e.g. 4 slot images)
        public Color activeSlotColor = Color.yellow;
        public Color emptySlotColor = Color.gray;

        [Header("Charm Item Item Container")]
        public Transform charmsContainer;       // Grid / Vertical layout for charm items
        public GameObject charmCardPrefab;      // Prefab with icon, name, cost, button

        [Header("Simple Default UI (if no prefab)")]
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

            // 1. Update text
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

            // 3. Update buttons if using direct inspector button list
            List<CharmItem> allCharms = CharmManager.Instance.GetAllCharms();
            if (defaultCharmButtons != null && defaultCharmButtons.Length > 0)
            {
                for (int i = 0; i < defaultCharmButtons.Length && i < allCharms.Count; i++)
                {
                    Button btn = defaultCharmButtons[i];
                    if (btn == null) continue;

                    CharmItem item = allCharms[i];
                    bool isEquipped = CharmManager.Instance.IsEquipped(item.id);
                    bool canEquip = CharmManager.Instance.CanEquip(item);

                    // Update button state / text
                    TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = isEquipped ? $"{item.charmName} (ถอดออก)" : $"{item.charmName} ({item.notchCost} ช่อง)";
                    }

                    btn.interactable = isEquipped || canEquip;

                    // Bind click
                    string charmId = item.id;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ToggleCharm(charmId));
                }
            }
        }

        public void ToggleCharm(string charmId)
        {
            if (CharmManager.Instance == null) return;

            if (CharmManager.Instance.IsEquipped(charmId))
            {
                CharmManager.Instance.UnequipCharm(charmId);
            }
            else
            {
                CharmManager.Instance.EquipCharm(charmId);
            }
        }
    }
}
