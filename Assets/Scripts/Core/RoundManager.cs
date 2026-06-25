using NarutoAutoBattle.AI;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Economy;
using UnityEngine;

namespace NarutoAutoBattle.Core
{
    public class RoundManager : MonoBehaviour
    {
        [SerializeField] BoardGrid boardGrid;
        [SerializeField] PlayerGold playerGold;
        [SerializeField] PlayerEconomy playerEconomy;
        [SerializeField] ShopService shopService;
        [SerializeField] EnemyBoardGenerator enemyBoardGenerator;

        int roundNumber = 1;
        int playerLosses;

        public int RoundNumber => roundNumber;
        public int PlayerLosses => playerLosses;
        public RoundIncomeBreakdown LastRoundIncome =>
            playerEconomy != null ? playerEconomy.LastIncome : RoundIncomeBreakdown.Empty;

        public void StartNewRound()
        {
            if (playerEconomy != null)
            {
                var income = playerEconomy.GrantRoundIncome(playerGold.Gold);
                playerGold.AddGold(income.TotalGold);
                Debug.Log($"Round {roundNumber} income: {income}");
            }
            else if (playerEconomy?.Rules != null)
            {
                playerGold.AddGold(playerEconomy.Rules.baseRoundGold);
            }

            shopService.RefreshShop();
            boardGrid.ClearZone(GridZone.EnemyBoard);
            enemyBoardGenerator.GenerateEnemyBoard(roundNumber);
        }

        public void OnCombatFinished(UnitOwner winner)
        {
            playerEconomy?.RegisterCombatResult(winner == UnitOwner.Player);

            if (winner == UnitOwner.Player)
                Debug.Log($"Round {roundNumber} won!");
            else
            {
                playerLosses++;
                Debug.Log($"Round {roundNumber} lost. Losses: {playerLosses}");
            }

            roundNumber++;
        }

        public void ResetRun()
        {
            roundNumber = 1;
            playerLosses = 0;
            playerEconomy?.ResetRun();
        }
    }
}
