using System;
using UnityEngine;

namespace NarutoAutoBattle.Synergies
{
    [CreateAssetMenu(fileName = "ClanData", menuName = "Naruto Auto Battle/Clan Data")]
    public class ClanData : ScriptableObject
    {
        [Serializable]
        public struct Threshold
        {
            public int unitCount;
            [Range(0f, 1f)] public float attackBonus;
            [Range(0f, 1f)] public float healthBonus;
        }

        public string clanId;
        public string displayName;
        public Color clanColor = Color.white;
        public Threshold[] thresholds;
    }
}
