using System;
using System.Collections;
using System.Collections.Generic;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Synergies;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    public class CombatController : MonoBehaviour
    {
        [SerializeField] BoardGrid boardGrid;
        [SerializeField] GameObject combatUnitPrefab;
        [SerializeField] SynergyService synergyService;

        readonly List<CombatUnit> activeCombatants = new();

        public bool IsCombatRunning { get; private set; }
        public event Action<UnitOwner> OnCombatFinished;

        void Awake()
        {
            if (GetComponent<DamagePopupSpawner>() == null)
                gameObject.AddComponent<DamagePopupSpawner>();
        }

        public void StartCombat(IEnumerable<Unit> playerUnits, IEnumerable<Unit> enemyUnits)
        {
            if (IsCombatRunning)
                return;

            StartCoroutine(CombatRoutine(playerUnits, enemyUnits));
        }

        IEnumerator CombatRoutine(IEnumerable<Unit> playerUnits, IEnumerable<Unit> enemyUnits)
        {
            IsCombatRunning = true;
            activeCombatants.Clear();

            var playerBonuses = synergyService != null
                ? synergyService.GetBonusesForOwner(UnitOwner.Player)
                : SynergyBonuses.Identity;
            var enemyBonuses = synergyService != null
                ? synergyService.GetBonusesForOwner(UnitOwner.Enemy)
                : SynergyBonuses.Identity;

            foreach (var unit in playerUnits)
            {
                unit.SetInCombat(true);
                activeCombatants.Add(SpawnCombatUnit(unit, playerBonuses));
            }

            foreach (var unit in enemyUnits)
            {
                unit.SetInCombat(true);
                activeCombatants.Add(SpawnCombatUnit(unit, enemyBonuses));
            }

            while (true)
            {
                var alive = activeCombatants.FindAll(c => c != null && c.IsAlive);
                int playerAlive = alive.FindAll(c => c.Owner == UnitOwner.Player).Count;
                int enemyAlive = alive.FindAll(c => c.Owner == UnitOwner.Enemy).Count;

                if (playerAlive == 0 || enemyAlive == 0)
                {
                    var winner = playerAlive > 0 ? UnitOwner.Player : UnitOwner.Enemy;
                    CleanupCombat();
                    IsCombatRunning = false;
                    OnCombatFinished?.Invoke(winner);
                    yield break;
                }

                var snapshot = alive.ToArray();
                foreach (var combatant in snapshot)
                    combatant.Tick(Time.deltaTime, snapshot);

                yield return null;
            }
        }

        CombatUnit SpawnCombatUnit(Unit unit, SynergyBonuses bonuses)
        {
            var go = Instantiate(combatUnitPrefab, unit.transform.position, unit.transform.rotation);
            var combatUnit = go.GetComponent<CombatUnit>();
            combatUnit.Initialize(unit, bonuses);

            if (go.GetComponent<CombatFeedback>() == null)
                go.AddComponent<CombatFeedback>();

            if (go.GetComponent<WorldHealthBar>() == null)
                go.AddComponent<WorldHealthBar>();

            var renderer = go.GetComponent<Renderer>() ?? go.GetComponentInChildren<Renderer>();
            if (renderer != null && unit.Data != null)
                renderer.material.color = unit.Data.unitColor;

            go.transform.localScale = unit.transform.localScale;
            return combatUnit;
        }

        void CleanupCombat()
        {
            foreach (var combatant in activeCombatants)
            {
                if (combatant != null)
                    Destroy(combatant.gameObject);
            }

            activeCombatants.Clear();

            foreach (var unit in boardGrid.GetAllUnits())
            {
                if (unit != null)
                    unit.SetInCombat(false);
            }
        }
    }
}
