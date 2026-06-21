using NarutoAutoBattle.Units;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NarutoAutoBattle.Units
{
    public static class UnitPoolLoader
    {
        const string UnitDataFolder = "Assets/ScriptableObjects/Units";

        public static UnitData[] Resolve(UnitData[] assignedPool)
        {
            if (HasValidEntries(assignedPool))
                return assignedPool;

#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets("t:UnitData", new[] { UnitDataFolder });
            if (guids.Length > 0)
            {
                var units = new UnitData[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                    units[i] = AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                return units;
            }
#endif
            Debug.LogError($"No UnitData found. Add assets under {UnitDataFolder} or run Naruto Auto Battle > Setup MVP Scene.");
            return System.Array.Empty<UnitData>();
        }

        static bool HasValidEntries(UnitData[] pool)
        {
            if (pool == null || pool.Length == 0)
                return false;

            foreach (var unit in pool)
            {
                if (unit != null)
                    return true;
            }

            return false;
        }
    }
}
