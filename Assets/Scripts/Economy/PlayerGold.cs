using System;
using UnityEngine;

namespace NarutoAutoBattle.Economy
{
    public class PlayerGold : MonoBehaviour
    {
        [SerializeField] int startingGold = 10;

        int gold;

        public int Gold => gold;
        public event Action<int> OnGoldChanged;

        void Awake()
        {
            gold = startingGold;
        }

        public void ResetGold()
        {
            gold = startingGold;
            OnGoldChanged?.Invoke(gold);
        }

        public bool CanAfford(int cost) => gold >= cost;

        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost))
                return false;

            gold -= cost;
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        public void AddGold(int amount)
        {
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }
    }
}
