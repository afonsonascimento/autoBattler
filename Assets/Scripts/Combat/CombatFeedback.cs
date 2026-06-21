using System.Collections;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatFeedback : MonoBehaviour
    {
        [SerializeField] float hitFlashDuration = 0.12f;
        [SerializeField] Color hitFlashColor = new(1f, 0.85f, 0.85f);
        [SerializeField] float deathShrinkDuration = 0.35f;

        Health health;
        Renderer[] renderers;
        Color[] originalColors;
        bool dying;

        void Awake()
        {
            health = GetComponent<Health>();
            CacheBodyRenderers();
        }

        void CacheBodyRenderers()
        {
            var all = GetComponentsInChildren<Renderer>();
            var body = new System.Collections.Generic.List<Renderer>();

            foreach (var renderer in all)
            {
                if (IsHealthBarRenderer(renderer.transform))
                    continue;

                body.Add(renderer);
            }

            renderers = body.ToArray();
            CacheColors();
        }

        static bool IsHealthBarRenderer(Transform transform)
        {
            var current = transform;
            while (current != null)
            {
                if (current.name == "HealthBar")
                    return true;
                current = current.parent;
            }

            return false;
        }

        void OnEnable()
        {
            health.OnDamaged += HandleDamaged;
            health.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDeath -= HandleDeath;
        }

        void CacheColors()
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                    originalColors[i] = renderers[i].material.color;
            }
        }

        void HandleDamaged(int amount, int current, int max)
        {
            if (dying)
                return;

            DamagePopupSpawner.Instance?.Spawn(transform.position, amount);
            StopAllCoroutines();
            StartCoroutine(HitFlashRoutine());
        }

        void HandleDeath()
        {
            if (dying)
                return;

            dying = true;
            StartCoroutine(DeathRoutine());
        }

        IEnumerator HitFlashRoutine()
        {
            SetColor(hitFlashColor);
            yield return new WaitForSeconds(hitFlashDuration);
            RestoreColors();
        }

        IEnumerator DeathRoutine()
        {
            var startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < deathShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / deathShrinkDuration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }

        void SetColor(Color color)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                    renderers[i].material.color = color;
            }
        }

        void RestoreColors()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                    renderers[i].material.color = originalColors[i];
            }
        }
    }
}
