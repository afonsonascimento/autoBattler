using NarutoAutoBattle.AI;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Economy;
using UnityEngine;

namespace NarutoAutoBattle.Core
{
    public class RoundManager : MonoBehaviour
    {
        const int BaseRoundGold = 5;
        const int WinBonusGold = 1;

        [SerializeField] BoardGrid boardGrid;
        [SerializeField] PlayerGold playerGold;
        [SerializeField] ShopService shopService;
        [SerializeField] EnemyBoardGenerator enemyBoardGenerator;

        int roundNumber = 1;
        int playerLosses;

        public int RoundNumber => roundNumber;
        public int PlayerLosses => playerLosses;

        public void StartNewRound()
        {
            playerGold.AddGold(BaseRoundGold);
            shopService.RefreshShop();
            boardGrid.ClearZone(GridZone.EnemyBoard);
            enemyBoardGenerator.GenerateEnemyBoard(roundNumber);
        }

        public void OnCombatFinished(UnitOwner winner)
        {
            if (winner == UnitOwner.Player)
            {
                playerGold.AddGold(WinBonusGold);
                Debug.Log($"Round {roundNumber} won! +{WinBonusGold} bonus gold.");
            }
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
        }
    }
}
