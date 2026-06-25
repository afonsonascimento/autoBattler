using System.Collections;
using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using UnityEngine;

namespace NarutoAutoBattle.Units
{
    public class Unit : MonoBehaviour
    {
        const float StarScalePerLevel = 0.15f;
        const float BaseStarScale = 0.8f;
        const float StatMultiplierPerStar = 1.8f;
        public const float VisualLocalY = -1f;

        [SerializeField] UnitData data;
        [SerializeField] Renderer meshRenderer;
        [SerializeField] Transform starIndicator;

        GameObject visualInstance;

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
            ApplyFacing();
        }

        public static Quaternion GetFacingRotation(UnitOwner unitOwner)
        {
            return Quaternion.Euler(0f, unitOwner == UnitOwner.Enemy ? 180f : 0f, 0f);
        }

        public void SetSlot(GridSlot slot)
        {
            previousSlot = currentSlot;
            currentSlot = slot;

            if (slot != null)
                SnapToGround();
        }

        public void SetStarLevel(int stars)
        {
            starLevel = Mathf.Clamp(stars, 1, 3);
            ApplyVisuals();
            SnapToGround();
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
            var baseScale = Vector3.one * GetStarScale();
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
            SnapToGround();
            mergeAnimation = null;
        }

        public void SetInCombat(bool inCombat)
        {
            isInCombat = inCombat;
            SetVisible(!inCombat);
        }

        void SetVisible(bool visible)
        {
            if (!visible)
            {
                foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
            }
            else
            {
                ApplyRendererVisibility();
            }

            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = visible;
        }

        void ApplyRendererVisibility()
        {
            if (meshRenderer != null)
                meshRenderer.enabled = visualInstance == null;

            if (visualInstance != null)
            {
                foreach (var renderer in visualInstance.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = true;
            }

            if (starIndicator != null)
                starIndicator.gameObject.SetActive(starLevel > 1);
        }

        float GetStarMultiplier()
        {
            return Mathf.Pow(StatMultiplierPerStar, starLevel - 1);
        }

        float GetStarScale()
        {
            return BaseStarScale + (starLevel - 1) * StarScalePerLevel;
        }

        Vector3 GetGroundedPosition(Vector3 slotWorldPosition)
        {
            float lift = GetStarScale() - BaseStarScale;
            return slotWorldPosition + Vector3.up * lift;
        }

        void SnapToGround()
        {
            if (currentSlot == null)
                return;

            transform.position = GetGroundedPosition(currentSlot.WorldPosition);
        }

        void ApplyVisuals()
        {
            if (visualInstance != null)
            {
                Destroy(visualInstance);
                visualInstance = null;
            }

            transform.localScale = Vector3.one * GetStarScale();

            if (data != null && data.visualPrefab != null)
            {
                visualInstance = Instantiate(data.visualPrefab, transform);
                visualInstance.transform.localPosition = new Vector3(data.visualOffset.x, VisualLocalY, data.visualOffset.z);
                visualInstance.transform.localRotation = Quaternion.identity;
                visualInstance.transform.localScale = Vector3.one * data.visualScale;
            }

            ApplyRendererVisibility();

            if (visualInstance == null && meshRenderer != null && data != null)
                meshRenderer.material.color = data.unitColor;

            if (starIndicator != null)
                starIndicator.localScale = Vector3.one * (1f + (starLevel - 1) * StarScalePerLevel);
        }

        void ApplyFacing()
        {
            transform.rotation = GetFacingRotation(owner);
        }

        void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<Renderer>();

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
