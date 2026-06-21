using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Economy
{
    public class UnitSellService : MonoBehaviour
    {
        [SerializeField] PlayerGold playerGold;
        [SerializeField] GameManager gameManager;

        void Awake()
        {
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GameManager>();
        }

        public bool CanSell => gameManager != null && gameManager.CurrentPhase == GamePhase.Prep;

        public bool TrySell(Unit unit)
        {
            if (!CanSell || unit == null || unit.Owner != UnitOwner.Player || unit.IsInCombat)
                return false;

            if (unit.Data == null)
                return false;

            int refund = GetSellValue(unit);
            var soldName = unit.Data.displayName;
            ClearUnitFromSlot(unit);
            Destroy(unit.gameObject);
            playerGold.AddGold(refund);
            Debug.Log($"Sold {soldName} for {refund}g.");
            return true;
        }

        public int GetSellValue(Unit unit)
        {
            return unit.Data.cost * unit.StarLevel;
        }

        static void ClearUnitFromSlot(Unit unit)
        {
            if (unit.CurrentSlot != null)
                unit.CurrentSlot.Occupant = null;
        }
    }
}
