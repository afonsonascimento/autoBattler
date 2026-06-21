using NarutoAutoBattle.Core;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    [RequireComponent(typeof(Health))]
    public class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] float heightOffset = 1.6f;
        [SerializeField] float barWidth = 1f;
        [SerializeField] float barHeight = 0.1f;

        static readonly Color BackgroundColor = new(0.1f, 0.1f, 0.1f, 0.9f);
        static readonly Color PlayerFillColor = new(0.25f, 0.85f, 0.35f);
        static readonly Color EnemyFillColor = new(0.9f, 0.25f, 0.25f);

        Health health;
        Transform barRoot;
        Transform fillTransform;
        Material fillMaterial;
        Color fillColor;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void Start()
        {
            BuildBarVisuals();
            UpdateBar(health.CurrentHealth, health.MaxHealth);
        }

        void OnEnable()
        {
            health.OnHealthChanged += UpdateBar;
            health.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            health.OnHealthChanged -= UpdateBar;
            health.OnDeath -= HandleDeath;
        }

        void LateUpdate()
        {
            if (barRoot == null)
                return;

            var cam = Camera.main;
            if (cam != null)
                barRoot.rotation = Quaternion.LookRotation(barRoot.position - cam.transform.position);
        }

        void BuildBarVisuals()
        {
            if (barRoot != null)
                return;

            var combatUnit = GetComponent<CombatUnit>();
            fillColor = combatUnit != null && combatUnit.Owner == UnitOwner.Enemy
                ? EnemyFillColor
                : PlayerFillColor;

            barRoot = new GameObject("HealthBar").transform;
            barRoot.SetParent(transform, false);
            barRoot.localPosition = new Vector3(0f, heightOffset, 0f);

            CreateBarQuad("Background", barRoot, BackgroundColor, barWidth, barHeight, 0f);

            fillTransform = CreateBarQuad("Fill", barRoot, fillColor, barWidth, barHeight * 0.75f, -0.001f);
            fillMaterial = fillTransform.GetComponent<Renderer>().material;
        }

        static Transform CreateBarQuad(string name, Transform parent, Color color, float width, float height, float zOffset)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Destroy(quad.GetComponent<Collider>());

            var quadTransform = quad.transform;
            quadTransform.SetParent(parent, false);
            quadTransform.localPosition = new Vector3(0f, 0f, zOffset);
            quadTransform.localScale = new Vector3(width, height, 1f);

            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            quad.GetComponent<Renderer>().material = material;

            return quadTransform;
        }

        void UpdateBar(int current, int max)
        {
            if (barRoot == null)
                return;

            if (fillTransform == null || max <= 0)
                return;

            float ratio = Mathf.Clamp01((float)current / max);
            fillTransform.localScale = new Vector3(barWidth * ratio, barHeight * 0.75f, 1f);
            fillTransform.localPosition = new Vector3(barWidth * (ratio - 1f) * 0.5f, 0f, -0.001f);

            if (fillMaterial != null)
                fillMaterial.color = Color.Lerp(new Color(0.9f, 0.2f, 0.1f), fillColor, ratio);
        }

        void HandleDeath()
        {
            if (barRoot != null)
                barRoot.gameObject.SetActive(false);
        }
    }
}
