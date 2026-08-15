using System;
using System.Collections.Generic;
using UnityEngine;

namespace Charms
{
    public enum CharmEffectType
    {
        FireballHealBoost,  // เก็บลูกไฟได้เลือดเยอะขึ้น 50% (Cost 2)
        RabbitDurationAdd,  // เพิ่มระยะเวลาของอาคมกระต่าย 10 วิ (Cost 1)
        WolfSummonAdd,      // เพิ่มจำนวนการอัญเชิญหมาป่า 1 ตัว (Cost 1)
        DeerRegenBoost,     // เพิ่มอัตราการฟื้นฟูของอาคมกวาง 50% (Cost 1)
        BullDurationAdd,    // เพิ่มระยะเวลาของอาคมวัว 5 วิ (Cost 1)
        BaseAttackBoost,    // เพิ่มพลังการโจมตีพื้นฐาน 50% (Cost 2)
        AttackSpeedBoost    // เพิ่มความเร็วในการโจมตี 50% (Cost 2)
    }

    [Serializable]
    public class CharmItem
    {
        public string id;
        public string charmName;
        [TextArea(2, 4)]
        public string description;
        public int notchCost = 1;
        public CharmEffectType effectType;
        public float value = 0.5f;
        public Sprite icon;
    }

    [CreateAssetMenu(fileName = "CharmDatabase", menuName = "Charms/Charm Database", order = 1)]
    public class CharmDatabaseSO : ScriptableObject
    {
        public List<CharmItem> charms = new List<CharmItem>();
    }
}
