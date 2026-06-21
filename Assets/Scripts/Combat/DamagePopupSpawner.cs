using System.Collections;
using UnityEngine;

namespace NarutoAutoBattle.Combat
{
    public class DamagePopupSpawner : MonoBehaviour
    {
        public static DamagePopupSpawner Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Spawn(Vector3 worldPosition, int damage)
        {
            StartCoroutine(AnimatePopup(worldPosition, damage.ToString(), new Color(1f, 0.35f, 0.2f)));
        }

        public void SpawnHeal(Vector3 worldPosition, int amount)
        {
            StartCoroutine(AnimatePopup(worldPosition, $"+{amount}", new Color(0.3f, 1f, 0.4f)));
        }

        public void SpawnLabel(Vector3 worldPosition, string text, Color color)
        {
            StartCoroutine(AnimatePopup(worldPosition, text, color));
        }

        IEnumerator AnimatePopup(Vector3 worldPosition, string text, Color color)
        {
            var go = new GameObject("DamagePopup");
            go.transform.position = worldPosition + Vector3.up * 1.5f;

            var textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            textMesh.fontStyle = FontStyle.Bold;

            var cam = Camera.main;
            if (cam != null)
                go.transform.rotation = Quaternion.LookRotation(go.transform.position - cam.transform.position);

            const float duration = 0.9f;
            float elapsed = 0f;
            var startPos = go.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                go.transform.position = startPos + Vector3.up * t * 1.2f;

                var c = color;
                c.a = 1f - t;
                textMesh.color = c;

                if (cam != null)
                    go.transform.rotation = Quaternion.LookRotation(go.transform.position - cam.transform.position);

                yield return null;
            }

            Destroy(go);
        }
    }
}
