using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.AI
{
    public class EnemyBoardGenerator : MonoBehaviour
    {
        [SerializeField] BoardGrid boardGrid;
        [SerializeField] UnitSpawner unitSpawner;
        [SerializeField] UnitData[] unitPool;

        public void GenerateEnemyBoard(int roundNumber)
        {
            EnsureUnitPool();
            boardGrid.ClearZone(GridZone.EnemyBoard);

            if (unitPool == null || unitPool.Length == 0)
                return;

            int unitCount = 2 + roundNumber / 2;

            for (int i = 0; i < unitCount; i++)
            {
                var slot = boardGrid.GetFirstEmptyEnemySlot();
                if (slot == null)
                    break;

                var data = unitPool[Random.Range(0, unitPool.Length)];
                unitSpawner.Spawn(data, slot, UnitOwner.Enemy, boardGrid);
            }
        }

        void EnsureUnitPool()
        {
            unitPool = UnitPoolLoader.Resolve(unitPool);
        }
    }
}
