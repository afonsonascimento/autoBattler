namespace NarutoAutoBattle.Core
{
    public enum GamePhase
    {
        Prep,
        Combat,
        RoundEnd,
        GameOver
    }

    public enum UnitOwner
    {
        Player,
        Enemy
    }

    public enum GridZone
    {
        PlayerBoard,
        EnemyBoard,
        Bench
    }
}
