using NarutoAutoBattle.Synergies;
using UnityEngine;

namespace NarutoAutoBattle.Units
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Naruto Auto Battle/Unit Data")]
    public class UnitData : ScriptableObject
    {
        public string unitId;
        public string displayName;
        public int cost = 1;
        public int baseHealth = 50;
        public int baseAttack = 10;
        public float attackRange = 1.5f;
        public float attackSpeed = 1f;
        public float moveSpeed = 3f;
        public Color unitColor = Color.white;
        public GameObject visualPrefab;
        public float visualScale = 1f;
        public Vector3 visualOffset;
        public ClanData[] clans;

        [Header("Jutsu")]
        public JutsuType jutsuType = JutsuType.None;
        public string jutsuName;
        public float jutsuCooldown = 4f;
        public float jutsuPower = 1.5f;
        public float jutsuRadius = 2.5f;
    }
}
