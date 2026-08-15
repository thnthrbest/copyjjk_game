using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Charms
{
    /// <summary>
    /// สคริปต์สำหรับดึงภาพเครื่องราง (Charm Icons) ที่สวมใส่อยู่มาแสดงผลบน UI / HUD ในฉากเล่นเกม (เช่น stage_1)
    /// สามารถใช้งานได้ทั้งแบบลาก Image Slots ที่วาง Layout ไว้แล้วมาใส่ หรือใช้กับ Container Layout
    /// </summary>
    public class EquippedCharmsHUDUI : MonoBehaviour
    {
        [Header("แบบที่ 1: ลาก Image Slots ที่วาง Layout ไว้แล้วมาใส่")]
        [Tooltip("ลากคอมโพเนนต์ Image ของสล็อตต่างๆ ใน Layout มาใส่ในลิสต์นี้ (เช่น Slot 1, Slot 2, Slot 3, Slot 4)")]
        public Image[] charmImageSlots;

        [Header("แบบที่ 2: ใช้กับ Container Layout (Dynamic Spawning)")]
        [Tooltip("ลาก Parent Layout Container (เช่น HorizontalLayoutGroup) มาใส่")]
        public Transform containerLayout;
        [Tooltip("Prefab ของ Image (ถ้าไม่ใส่ ระบบจะสร้าง UI Image ให้เองอัตโนมัติ)")]
        public GameObject imagePrefab;

        [Header("การตั้งค่าการแสดงผล")]
        [Tooltip("ซ่อนสล็อตที่ว่างเปล่าหรือไม่ (ถ้า true จะ setActive(false) สล็อตที่ไม่มีไอเทม)")]
        public bool hideEmptySlots = true;

        private void OnEnable()
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.OnCharmsChanged += RefreshEquippedCharms;
            }
            RefreshEquippedCharms();
        }

        private void OnDisable()
        {
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.OnCharmsChanged -= RefreshEquippedCharms;
            }
        }

        private void Start()
        {
            RefreshEquippedCharms();
        }

        /// <summary>
        /// ดึงข้อมูลเครื่องรางที่สวมใส่อยู่จาก CharmManager แล้วอัปเดตแสดงผลเฉพาะรูปภาพ (Image/Sprite)
        /// </summary>
        public void RefreshEquippedCharms()
        {
            if (CharmManager.Instance == null) return;

            // 1. ดึง ID เครื่องรางทั้งหมดที่สวมใส่อยู่ในปัจจุบัน
            List<string> equippedIds = CharmManager.Instance.GetEquippedCharmIdsList();
            List<Sprite> equippedIcons = new List<Sprite>();

            foreach (string id in equippedIds)
            {
                CharmItem charm = CharmManager.Instance.GetCharmById(id);
                if (charm != null && charm.icon != null)
                {
                    equippedIcons.Add(charm.icon);
                }
            }

            // 2. อัปเดตช่อง Image Slots ตาม Layout ที่วางไว้
            if (charmImageSlots != null && charmImageSlots.Length > 0)
            {
                for (int i = 0; i < charmImageSlots.Length; i++)
                {
                    if (charmImageSlots[i] == null) continue;

                    if (i < equippedIcons.Count && equippedIcons[i] != null)
                    {
                        charmImageSlots[i].sprite = equippedIcons[i];
                        charmImageSlots[i].enabled = true;
                        charmImageSlots[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        charmImageSlots[i].sprite = null;
                        if (hideEmptySlots)
                        {
                            charmImageSlots[i].enabled = false;
                            charmImageSlots[i].gameObject.SetActive(false);
                        }
                        else
                        {
                            charmImageSlots[i].enabled = false;
                        }
                    }
                }
            }

            // 3. อัปเดต Container Layout (กรณีสร้างไอคอนใหม่ตามจำนวนที่ใส่)
            if (containerLayout != null)
            {
                // ลบ Object เก่าออก
                foreach (Transform child in containerLayout)
                {
                    Destroy(child.gameObject);
                }

                // สร้าง Image Object สำหรับทุกเครื่องรางที่สวมใส่อยู่
                foreach (Sprite icon in equippedIcons)
                {
                    if (icon == null) continue;

                    GameObject iconObj;
                    if (imagePrefab != null)
                    {
                        iconObj = Instantiate(imagePrefab, containerLayout);
                    }
                    else
                    {
                        iconObj = new GameObject("EquippedCharmIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        iconObj.transform.SetParent(containerLayout, false);
                        RectTransform rect = iconObj.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(50, 50);
                    }

                    Image img = iconObj.GetComponent<Image>();
                    if (img == null) img = iconObj.GetComponentInChildren<Image>();

                    if (img != null)
                    {
                        img.sprite = icon;
                        img.enabled = true;
                    }
                }
            }
        }
    }
}
