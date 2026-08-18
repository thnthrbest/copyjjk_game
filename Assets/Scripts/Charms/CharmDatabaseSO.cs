using System.Collections.Generic;
using UnityEngine;

namespace Charms
{
    [CreateAssetMenu(fileName = "CharmDatabase", menuName = "Charms/Charm Database", order = 1)]
    public class CharmDatabaseSO : ScriptableObject
    {
        public List<CharmItem> charms = new List<CharmItem>();
    }
}
