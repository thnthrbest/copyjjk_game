using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Charms
{
    public class CharmCardUI : MonoBehaviour
    {
        [Header("Card Components")]
        public Image iconImage;               // Icon เครื่องราง
        public TextMeshProUGUI nameText;       // ชื่อเครื่องราง
        public TextMeshProUGUI descText;       // คำอธิบาย
        public TextMeshProUGUI notchCostText;  // จำนวนช่องที่ต้องใช้ (notchCost)
        public TextMeshProUGUI ownedCountText; // จำนวนที่มีอยู่ / จำนวนที่ใส่แล้ว

        [Header("Buttons")]
        public Button equipButton;            // ปุ่มกดสวมใส่ (+)
        public TextMeshProUGUI equipBtnText;   // ข้อความบนปุ่มใส่
        public Button unequipButton;          // ปุ่มกดถอดออก (-) [Optional]

        private CharmItem currentCharm;

        public void SetupCard(CharmItem charm)
        {
            currentCharm = charm;
            if (charm == null || CharmManager.Instance == null) return;

            // 1. Icon
            if (iconImage != null)
            {
                if (charm.icon != null)
                {
                    iconImage.sprite = charm.icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // 2. Name
            if (nameText != null)
            {
                nameText.text = charm.charmName;
            }

            // 3. Description
            if (descText != null)
            {
                descText.text = charm.description;
            }

            // 4. Notch Cost
            if (notchCostText != null)
            {
                notchCostText.text = $"ใช้ช่อง: {charm.notchCost}";
            }

            // 5. Owned & Equipped Count
            int ownedCount = CharmManager.Instance.GetOwnedCount(charm.id);
            int equippedCount = CharmManager.Instance.GetEquippedCount(charm.id);

            if (ownedCountText != null)
            {
                ownedCountText.text = $"มีอยู่: {ownedCount} ชิ้น (ใส่แล้ว {equippedCount})";
            }

            // 6. Equip / Unequip Buttons Logic
            bool canEquipMore = CharmManager.Instance.CanEquip(charm);

            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(() =>
                {
                    CharmManager.Instance.EquipCharm(charm.id);
                });

                equipButton.interactable = canEquipMore;

                if (equipBtnText != null)
                {
                    equipBtnText.text = "สวมใส่ (+)";
                }
            }

            if (unequipButton != null)
            {
                unequipButton.onClick.RemoveAllListeners();
                unequipButton.onClick.AddListener(() =>
                {
                    CharmManager.Instance.UnequipCharm(charm.id);
                });

                unequipButton.interactable = equippedCount > 0;
            }
        }
    }
}
