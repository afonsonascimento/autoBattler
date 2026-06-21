using System.Collections;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using UnityEngine;

namespace NarutoAutoBattle.Units
{
    public class Unit : MonoBehaviour
    {
        const float StarScalePerLevel = 0.15f;
        const float StatMultiplierPerStar = 1.8f;

        [SerializeField] UnitData data;
        [SerializeField] Renderer meshRenderer;
        [SerializeField] Transform starIndicator;

        UnitOwner owner = UnitOwner.Player;
        int starLevel = 1;
        GridSlot currentSlot;
        GridSlot previousSlot;
        bool isInCombat;
        Coroutine mergeAnimation;

        public UnitData Data => data;
        public UnitOwner Owner => owner;
        public int StarLevel => starLevel;
        public GridSlot CurrentSlot => currentSlot;
        public GridSlot PreviousSlot => previousSlot;
        public bool IsInCombat => isInCombat;

        public int MaxHealth => Mathf.RoundToInt(data.baseHealth * GetStarMultiplier());
        public int Attack => Mathf.RoundToInt(data.baseAttack * GetStarMultiplier());
        public float AttackRange => data.attackRange;
        public float AttackSpeed => data.attackSpeed;
        public float MoveSpeed => data.moveSpeed;

        public void Initialize(UnitData unitData, UnitOwner unitOwner, int stars = 1)
        {
            data = unitData;
            owner = unitOwner;
            starLevel = Mathf.Clamp(stars, 1, 3);
            ApplyVisuals();
        }

        public void SetSlot(GridSlot slot)
        {
            previousSlot = currentSlot;
            currentSlot = slot;

            if (slot != null)
                transform.position = slot.WorldPosition;
        }

        public void SetStarLevel(int stars)
        {
            starLevel = Mathf.Clamp(stars, 1, 3);
            ApplyVisuals();
            PlayMergeAnimation();
        }

        public void PlayMergeAnimation()
        {
            if (mergeAnimation != null)
                StopCoroutine(mergeAnimation);

            mergeAnimation = StartCoroutine(MergePunchRoutine());
        }

        IEnumerator MergePunchRoutine()
        {
            var baseScale = Vector3.one * (0.8f + (starLevel - 1) * StarScalePerLevel);
            var punchScale = baseScale * 1.35f;
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(punchScale, baseScale, t);
                yield return null;
            }

            transform.localScale = baseScale;
            mergeAnimation = null;
        }

        public void SetInCombat(bool inCombat)
        {
            isInCombat = inCombat;
            SetVisible(!inCombat);
        }

        void SetVisible(bool visible)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                renderer.enabled = visible;

            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = visible;
        }

        float GetStarMultiplier()
        {
            return Mathf.Pow(StatMultiplierPerStar, starLevel - 1);
        }

        void ApplyVisuals()
        {
            if (meshRenderer != null)
            {
                var mat = meshRenderer.material;
                mat.color = data != null ? data.unitColor : Color.white;
            }

            if (starIndicator != null)
            {
                starIndicator.gameObject.SetActive(starLevel > 1);
                starIndicator.localScale = Vector3.one * (1f + (starLevel - 1) * StarScalePerLevel);
            }

            transform.localScale = Vector3.one * (0.8f + (starLevel - 1) * StarScalePerLevel);
        }

        void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponentInChildren<Renderer>();

            if (starIndicator == null)
            {
                var starTransform = transform.Find("StarIndicator");
                if (starTransform != null)
                    starIndicator = starTransform;
            }

            ApplyVisuals();
        }
    }
}
