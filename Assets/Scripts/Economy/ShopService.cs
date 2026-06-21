using System;
using System.Collections.Generic;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.Economy
{
    public class ShopService : MonoBehaviour
    {
        const int ShopSize = 5;
        const int RerollCost = 2;

        [SerializeField] UnitData[] unitPool;
        [SerializeField] UnitSpawner unitSpawner;
        [SerializeField] BenchGrid benchGrid;
        [SerializeField] PlayerGold playerGold;

        readonly List<UnitData> currentOffers = new();

        public IReadOnlyList<UnitData> CurrentOffers => currentOffers;
        public int RerollCostAmount => RerollCost;
        public int ShopSlotCount => ShopSize;

        public event Action OnShopChanged;

        void Awake()
        {
            EnsureUnitPool();
            RefreshShop();
        }

        void EnsureUnitPool()
        {
            unitPool = UnitPoolLoader.Resolve(unitPool);
        }

        public void RefreshShop()
        {
            currentOffers.Clear();
            EnsureUnitPool();

            if (unitPool == null || unitPool.Length == 0)
            {
                OnShopChanged?.Invoke();
                return;
            }

            for (int i = 0; i < ShopSize; i++)
                currentOffers.Add(RollRandomOffer());

            OnShopChanged?.Invoke();
        }

        public bool TryBuy(int index)
        {
            if (index < 0 || index >= currentOffers.Count)
                return false;

            var data = currentOffers[index];
            if (data == null || !playerGold.TrySpend(data.cost))
                return false;

            var slot = benchGrid.GetFirstEmptySlot();
            if (slot == null)
            {
                playerGold.AddGold(data.cost);
                Debug.LogWarning("Bench is full.");
                return false;
            }

            unitSpawner.Spawn(data, slot, UnitOwner.Player, benchGrid);
            UnitMergeService.Instance?.ProcessMerges();

            currentOffers[index] = RollRandomOffer();
            OnShopChanged?.Invoke();
            return true;
        }

        public bool TryReroll()
        {
            if (!playerGold.TrySpend(RerollCost))
                return false;

            RefreshShop();
            return true;
        }

        UnitData RollRandomOffer()
        {
            if (unitPool == null || unitPool.Length == 0)
                return null;

            return unitPool[UnityEngine.Random.Range(0, unitPool.Length)];
        }
    }
}
