using NarutoAutoBattle.Core;
using NarutoAutoBattle.Synergies;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatUnit : MonoBehaviour
    {
        Unit sourceUnit;
        Health health;
        CombatUnit currentTarget;
        float attackCooldown;
        float jutsuCooldown;
        bool active;
        int attackPower;

        public Unit SourceUnit => sourceUnit;
        public UnitOwner Owner => sourceUnit.Owner;
        public Health Health => health;
        public bool IsAlive => health.IsAlive;
        public bool Active => active;

        public int Attack => attackPower;
        public float AttackRange => sourceUnit.AttackRange;
        public float AttackSpeed => sourceUnit.AttackSpeed;
        public float MoveSpeed => sourceUnit.MoveSpeed;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        public void Initialize(Unit unit, SynergyBonuses bonuses)
        {
            sourceUnit = unit;
            attackPower = Mathf.RoundToInt(unit.Attack * bonuses.AttackMultiplier);
            int maxHealth = Mathf.RoundToInt(unit.MaxHealth * bonuses.HealthMultiplier);
            health.Initialize(maxHealth);
            attackCooldown = 0f;
            jutsuCooldown = unit.Data != null && unit.Data.jutsuType != JutsuType.None
                ? unit.Data.jutsuCooldown * 0.5f
                : 0f;
            active = true;
        }

        public void SetActive(bool isActive)
        {
            active = isActive;
        }

        public void Tick(float deltaTime, CombatUnit[] allUnits)
        {
            if (!active || !IsAlive)
                return;

            attackCooldown -= deltaTime;
            jutsuCooldown -= deltaTime;

            if (JutsuExecutor.TryCast(this, allUnits, ref jutsuCooldown))
                return;

            if (currentTarget == null || !currentTarget.IsAlive)
                currentTarget = FindNearestEnemy(allUnits);

            if (currentTarget == null)
                return;

            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist > AttackRange)
            {
                var dir = (currentTarget.transform.position - transform.position).normalized;
                dir.y = 0f;
                transform.position += dir * MoveSpeed * deltaTime;
                transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));
            }
            else if (attackCooldown <= 0f)
            {
                currentTarget.health.TakeDamage(Attack);
                attackCooldown = 1f / AttackSpeed;
            }
        }

        CombatUnit FindNearestEnemy(CombatUnit[] allUnits)
        {
            CombatUnit nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var other in allUnits)
            {
                if (other == null || !other.IsAlive || other.Owner == Owner)
                    continue;

                float dist = Vector3.SqrMagnitude(transform.position - other.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = other;
                }
            }

            return nearest;
        }
    }
}
