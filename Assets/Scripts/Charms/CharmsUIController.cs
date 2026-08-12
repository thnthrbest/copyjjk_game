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

        [Header("Vertical Scroll View UI")]
        [Tooltip("Content Transform ของ Scroll View สำหรับวางเรียง Card เครื่องรางลงมาตามแนวตั้ง")]
        public Transform charmsContentContainer; // Content Object ใน Scroll View
        [Tooltip("Prefab การ์ดเครื่องราง (ต้องมีคอมโพเนนต์ CharmCardUI)")]
        public GameObject charmCardPrefab;      // Card Prefab

        [Header("Empty Inventory Warning UI")]
        public GameObject emptyInventoryTextObject; // แสดงเมื่อผู้เล่นยังไม่มีเครื่องรางในคลังเลย

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

            // 1. อัปเดตข้อความแสดงจำนวน Notch
            if (notchStatusText != null)
            {
                notchStatusText.text = $"ช่องเครื่องรางที่ใช้: {used} / {max}";
            }

            // 2. อัปเดตสล็อตไอคอน Notch
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

            // 3. สร้าง Card แสดงเครื่องรางที่มีอยู่ใน Vertical Scroll View
            RenderOwnedCharmCards();
        }

        /// <summary>
        /// ดึงเครื่องรางที่มีอยู่ (GetOwnedCount > 0) มาสร้างเป็น Card เรียงใน Vertical Scroll View
        /// </summary>
        private void RenderOwnedCharmCards()
        {
            if (charmsContentContainer == null || charmCardPrefab == null) return;

            // ลบ Card เก่าออกก่อน
            foreach (Transform child in charmsContentContainer)
            {
                Destroy(child.gameObject);
            }

            List<CharmItem> allCharms = CharmManager.Instance.GetAllCharms();
            int renderedCount = 0;

            foreach (CharmItem item in allCharms)
            {
                int ownedCount = CharmManager.Instance.GetOwnedCount(item.id);

                // **แสดงเฉพาะเครื่องรางที่มีอยู่ในคลังเท่านั้น (ownedCount > 0)**
                if (ownedCount > 0)
                {
                    GameObject cardObj = Instantiate(charmCardPrefab, charmsContentContainer);
                    CharmCardUI cardUI = cardObj.GetComponent<CharmCardUI>();

                    if (cardUI != null)
                    {
                        cardUI.SetupCard(item);
                    }
                    else
                    {
                        Debug.LogWarning("[CharmsUIController] charmCardPrefab ไม่มีคอมโพเนนต์ CharmCardUI!");
                    }

                    renderedCount++;
                }
            }

            // แสดงข้อความแจ้งเตือนถ้ายังไม่มีเครื่องรางเลย
            if (emptyInventoryTextObject != null)
            {
                emptyInventoryTextObject.SetActive(renderedCount == 0);
            }
        }
    }
}
