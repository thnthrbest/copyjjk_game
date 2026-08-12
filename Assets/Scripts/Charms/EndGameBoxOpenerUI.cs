using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Charms
{
    public class EndGameBoxOpenerUI : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject unboxingPanel;           // Panel สำหรับแสดงหน้าเปิดกล่องเครื่องราง
        public GameObject mainEndGamePanel;        // Panel สรุปผลเกมปกติ ( Victory / Game Over )

        [Header("Box Status UI")]
        public TextMeshProUGUI boxesRemainingText;  // Text แสดงจำนวนกล่องที่เหลืออยู่
        public Button openBoxButton;               // ปุ่มกดเปิดทีละกล่อง
        public Button openAllButton;               // ปุ่มกดเปิดกล่องทั้งหมด

        [Header("Pulled Result Display UI")]
        public GameObject resultDisplayContainer;  // UI Card แสดงผลลัพธ์เครื่องรางที่สุ่มได้
        public Image charmIconImage;               // รูปไอคอนเครื่องราง
        public TextMeshProUGUI charmNameText;      // ชื่อเครื่องราง
        public TextMeshProUGUI charmDescText;      // คำอธิบายเครื่องราง
        public TextMeshProUGUI charmOwnedCountText; // จำนวนที่ครอบครองทั้งหมด (เช่น "ครอบครองแล้ว: 3 ชิ้น")
        public Button continueButton;              // ปุ่มกดเพื่อไปยังกล่องถัดไป / ไปยังหน้าจบเกม

        [Header("SFX")]
        public AudioClip openBoxSFX;

        private void OnEnable()
        {
            CheckAndStartUnboxingSequence();
        }

        /// <summary>
        /// เช็คว่ามีกล่องจากการเล่นในรอบนี้หรือไม่ ถ้ามีจะเปิดหน้าต่างบังคับเปิดกล่อง
        /// </summary>
        public void CheckAndStartUnboxingSequence()
        {
            if (CharmManager.Instance == null)
            {
                ShowMainEndGamePanel();
                return;
            }

            int boxesCount = CharmManager.Instance.GetRunBoxesCount();
            if (boxesCount > 0)
            {
                if (unboxingPanel != null) unboxingPanel.SetActive(true);
                if (mainEndGamePanel != null) mainEndGamePanel.SetActive(false);
                if (resultDisplayContainer != null) resultDisplayContainer.SetActive(false);

                UpdateBoxesRemainingText(boxesCount);

                if (openBoxButton != null)
                {
                    openBoxButton.onClick.RemoveAllListeners();
                    openBoxButton.onClick.AddListener(OnOpenOneBoxClicked);
                    openBoxButton.gameObject.SetActive(true);
                }

                if (openAllButton != null)
                {
                    openAllButton.onClick.RemoveAllListeners();
                    openAllButton.onClick.AddListener(OnOpenAllBoxesClicked);
                    openAllButton.gameObject.SetActive(true);
                }
            }
            else
            {
                ShowMainEndGamePanel();
            }
        }

        public void OnOpenOneBoxClicked()
        {
            if (CharmManager.Instance == null) return;

            CharmItem item = CharmManager.Instance.OpenOneBox();
            if (item != null)
            {
                DisplayPulledCharmResult(item);
            }

            int remaining = CharmManager.Instance.GetRunBoxesCount();
            UpdateBoxesRemainingText(remaining);
        }

        public void OnOpenAllBoxesClicked()
        {
            if (CharmManager.Instance == null) return;

            int count = CharmManager.Instance.GetRunBoxesCount();
            CharmItem lastPulled = null;

            for (int i = 0; i < count; i++)
            {
                lastPulled = CharmManager.Instance.OpenOneBox();
            }

            if (lastPulled != null)
            {
                DisplayPulledCharmResult(lastPulled);
            }

            UpdateBoxesRemainingText(0);
        }

        private void DisplayPulledCharmResult(CharmItem item)
        {
            if (resultDisplayContainer != null) resultDisplayContainer.SetActive(true);

            if (charmIconImage != null && item.icon != null) charmIconImage.sprite = item.icon;
            if (charmNameText != null) charmNameText.text = item.charmName;
            if (charmDescText != null) charmDescText.text = item.description;

            int totalOwned = CharmManager.Instance.GetOwnedCount(item.id);
            if (charmOwnedCountText != null) charmOwnedCountText.text = $"ครอบครองแล้วทั้งหมด: {totalOwned} ชิ้น";

            int remainingBoxes = CharmManager.Instance.GetRunBoxesCount();
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();

                if (remainingBoxes > 0)
                {
                    if (btnText != null) btnText.text = $"เปิดกล่องถัดไป (เหลือ {remainingBoxes} กล่อง)";
                    continueButton.onClick.AddListener(() =>
                    {
                        resultDisplayContainer.SetActive(false);
                    });
                }
                else
                {
                    if (btnText != null) btnText.text = "ต่อไป (สรุปผลเกม)";
                    continueButton.onClick.AddListener(() =>
                    {
                        ShowMainEndGamePanel();
                    });
                }
            }

            if (openBoxSFX != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(openBoxSFX);
            }
        }

        private void UpdateBoxesRemainingText(int count)
        {
            if (boxesRemainingText != null)
            {
                boxesRemainingText.text = $"คุณได้กล่องเครื่องรางจากการเล่น: {count} กล่อง";
            }
        }

        private void ShowMainEndGamePanel()
        {
            if (unboxingPanel != null) unboxingPanel.SetActive(false);
            if (mainEndGamePanel != null) mainEndGamePanel.SetActive(true);
        }
    }
}
