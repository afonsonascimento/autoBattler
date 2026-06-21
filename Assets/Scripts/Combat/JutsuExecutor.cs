using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    public static class JutsuExecutor
    {
        public static bool TryCast(CombatUnit caster, CombatUnit[] allUnits, ref float jutsuCooldown)
        {
            var data = caster.SourceUnit?.Data;
            if (data == null || data.jutsuType == JutsuType.None || jutsuCooldown > 0f)
                return false;

            bool cast = data.jutsuType switch
            {
                JutsuType.Rasengan => CastRasengan(caster, allUnits, data),
                JutsuType.Chidori => CastChidori(caster, allUnits, data),
                JutsuType.MysticalPalm => CastMysticalPalm(caster, allUnits, data),
                JutsuType.LightningBlade => CastLightningBlade(caster, allUnits, data),
                _ => false
            };

            if (cast)
            {
                jutsuCooldown = data.jutsuCooldown;
                SpawnJutsuLabel(caster.transform.position, data.jutsuName);
            }

            return cast;
        }

        static bool CastRasengan(CombatUnit caster, CombatUnit[] allUnits, UnitData data)
        {
            if (!IsNearAnyEnemy(caster, allUnits, caster.AttackRange))
                return false;

            int damage = Mathf.RoundToInt(caster.Attack * data.jutsuPower);
            bool hit = false;

            foreach (var enemy in allUnits)
            {
                if (!IsEnemy(caster, enemy))
                    continue;

                if (Vector3.Distance(caster.transform.position, enemy.transform.position) <= data.jutsuRadius)
                {
                    enemy.Health.TakeDamage(damage);
                    hit = true;
                }
            }

            return hit;
        }

        static bool CastChidori(CombatUnit caster, CombatUnit[] allUnits, UnitData data)
        {
            var target = FindNearestEnemy(caster, allUnits);
            if (target == null || Vector3.Distance(caster.transform.position, target.transform.position) > caster.AttackRange)
                return false;

            int damage = Mathf.RoundToInt(caster.Attack * data.jutsuPower);
            target.Health.TakeDamage(damage);
            return true;
        }

        static bool CastMysticalPalm(CombatUnit caster, CombatUnit[] allUnits, UnitData data)
        {
            var ally = FindLowestHealthAlly(caster, allUnits);
            if (ally == null)
                return false;

            float healthRatio = (float)ally.Health.CurrentHealth / ally.Health.MaxHealth;
            if (healthRatio > 0.92f)
                return false;

            int healAmount = Mathf.RoundToInt(caster.Attack * data.jutsuPower);
            ally.Health.Heal(healAmount);
            DamagePopupSpawner.Instance?.SpawnHeal(ally.transform.position, healAmount);
            return true;
        }

        static bool CastLightningBlade(CombatUnit caster, CombatUnit[] allUnits, UnitData data)
        {
            var target = FindNearestEnemy(caster, allUnits);
            if (target == null || Vector3.Distance(caster.transform.position, target.transform.position) > data.jutsuRadius)
                return false;

            int damage = Mathf.RoundToInt(caster.Attack * data.jutsuPower);
            bool hit = false;

            foreach (var enemy in allUnits)
            {
                if (!IsEnemy(caster, enemy))
                    continue;

                if (Vector3.Distance(target.transform.position, enemy.transform.position) <= 1.8f)
                {
                    enemy.Health.TakeDamage(damage);
                    hit = true;
                }
            }

            return hit;
        }

        static bool IsNearAnyEnemy(CombatUnit caster, CombatUnit[] allUnits, float range)
        {
            foreach (var other in allUnits)
            {
                if (!IsEnemy(caster, other))
                    continue;

                if (Vector3.Distance(caster.transform.position, other.transform.position) <= range)
                    return true;
            }

            return false;
        }

        static CombatUnit FindNearestEnemy(CombatUnit caster, CombatUnit[] allUnits)
        {
            CombatUnit nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var other in allUnits)
            {
                if (!IsEnemy(caster, other))
                    continue;

                float dist = Vector3.SqrMagnitude(caster.transform.position - other.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = other;
                }
            }

            return nearest;
        }

        static CombatUnit FindLowestHealthAlly(CombatUnit caster, CombatUnit[] allUnits)
        {
            CombatUnit lowest = null;
            float lowestRatio = float.MaxValue;

            foreach (var other in allUnits)
            {
                if (!other.IsAlive || other.Owner != caster.Owner)
                    continue;

                float ratio = (float)other.Health.CurrentHealth / other.Health.MaxHealth;
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    lowest = other;
                }
            }

            return lowest;
        }

        static bool IsEnemy(CombatUnit caster, CombatUnit other)
        {
            return other != null && other.IsAlive && other.Owner != caster.Owner;
        }

        static void SpawnJutsuLabel(Vector3 position, string jutsuName)
        {
            if (string.IsNullOrEmpty(jutsuName))
                return;

            DamagePopupSpawner.Instance?.SpawnLabel(position + Vector3.up * 0.3f, jutsuName, new Color(0.4f, 0.85f, 1f));
        }
    }
}
