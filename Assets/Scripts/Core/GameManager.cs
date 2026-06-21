using NarutoAutoBattle.AI;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Combat;
using NarutoAutoBattle.Economy;
using NarutoAutoBattle.Input;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Core
{
    public class GameManager : MonoBehaviour
    {
        const int MaxLosses = 5;

        [SerializeField] BoardGrid boardGrid;
        [SerializeField] BenchGrid benchGrid;
        [SerializeField] PlayerGold playerGold;
        [SerializeField] ShopService shopService;
        [SerializeField] CombatController combatController;
        [SerializeField] RoundManager roundManager;
        [SerializeField] UnitDragController dragController;
        [SerializeField] EnemyBoardGenerator enemyBoardGenerator;

        GamePhase currentPhase = GamePhase.Prep;
        bool lastCombatWon;

        public GamePhase CurrentPhase => currentPhase;
        public bool LastCombatWon => lastCombatWon;
        public RoundManager RoundManager => roundManager;
        public PlayerGold PlayerGold => playerGold;

        public event System.Action<GamePhase> OnPhaseChanged;

        void Awake()
        {
            combatController.OnCombatFinished += HandleCombatFinished;
        }

        void Start()
        {
            SetPhase(GamePhase.Prep);
            enemyBoardGenerator.GenerateEnemyBoard(roundManager.RoundNumber);
        }

        void OnDestroy()
        {
            combatController.OnCombatFinished -= HandleCombatFinished;
        }

        public void StartFight()
        {
            if (currentPhase != GamePhase.Prep)
                return;

            if (!HasPlayerBoardUnits())
            {
                Debug.LogWarning("Place at least one unit on the board before fighting.");
                return;
            }

            SetPhase(GamePhase.Combat);
            dragController.DraggingEnabled = false;

            combatController.StartCombat(
                boardGrid.GetPlayerBoardUnits(),
                boardGrid.GetEnemyBoardUnits());
        }

        void HandleCombatFinished(UnitOwner winner)
        {
            lastCombatWon = winner == UnitOwner.Player;
            roundManager.OnCombatFinished(winner);
            SetPhase(GamePhase.RoundEnd);

            if (roundManager.PlayerLosses >= MaxLosses)
            {
                SetPhase(GamePhase.GameOver);
                Debug.Log("Game Over!");
                return;
            }

            SetPhase(GamePhase.Prep);
            roundManager.StartNewRound();
            dragController.DraggingEnabled = true;
        }

        void SetPhase(GamePhase phase)
        {
            currentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        bool HasPlayerBoardUnits()
        {
            foreach (var _ in boardGrid.GetPlayerBoardUnits())
                return true;
            return false;
        }
    }
}
