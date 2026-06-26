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
        [SerializeField] PlayerEconomy playerEconomy;

        readonly List<UnitData> currentOffers = new();
        readonly Dictionary<int, List<UnitData>> unitsByCost = new();

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
            RebuildCostBuckets();
        }

        void RebuildCostBuckets()
        {
            unitsByCost.Clear();

            if (unitPool == null)
                return;

            foreach (var unit in unitPool)
            {
                if (unit == null)
                    continue;

                if (!unitsByCost.TryGetValue(unit.cost, out var bucket))
                {
                    bucket = new List<UnitData>();
                    unitsByCost[unit.cost] = bucket;
                }

                bucket.Add(unit);
            }
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

            int level = playerEconomy != null ? playerEconomy.Level : 1;
            var rules = playerEconomy != null ? playerEconomy.Rules : null;
            int cost = rules != null ? rules.RollShopCost(level) : 1;
            return PickRandomUnitAtCost(cost) ?? unitPool[UnityEngine.Random.Range(0, unitPool.Length)];
        }

        UnitData PickRandomUnitAtCost(int cost)
        {
            if (unitsByCost.TryGetValue(cost, out var bucket) && bucket.Count > 0)
                return bucket[UnityEngine.Random.Range(0, bucket.Count)];

            for (int fallback = 1; fallback <= EconomyRules.MaxShopCost; fallback++)
            {
                if (unitsByCost.TryGetValue(fallback, out bucket) && bucket.Count > 0)
                    return bucket[UnityEngine.Random.Range(0, bucket.Count)];
            }

            return null;
        }
    }
}
