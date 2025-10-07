using _Scripts.Ships.Modules;
using DG.Tweening;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Common
{
    [RequireComponent(typeof(ShipModule))]
    public class ModuleVFXController : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private GameObject saleVFX;
        [SerializeField] private GameObject floatingTextPrefab;

        [Inject] private IPrefabPool prefabPool;

        private Health _health;
        private ShipModule _module;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _module = GetComponent<ShipModule>();
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.OnDeath += OnDestroyed;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDeath -= OnDestroyed;
        }

        // 💥 Called when destroyed by damage
        private void OnDestroyed(Health _)
        {
            PlayExplosion();
        }

        // 💰 Called externally when sold for points
        public void OnSold(int scoreValue)
        {
            PlaySale(scoreValue);
        }

        private void PlayExplosion()
        {
            if (explosionVFX != null)
                SpawnVFX(explosionVFX, 1.2f);
        }

        private void PlaySale(int scoreValue)
        {
            if (saleVFX != null)
                SpawnVFX(saleVFX, 1f);

            if (floatingTextPrefab != null)
                ShowFloatingText($"${scoreValue}$", Color.yellow);
        }

        private void SpawnVFX(GameObject prefab, float lifetime)
        {
            GameObject vfx = prefabPool.Spawn(prefab, transform.position, Quaternion.identity);
            vfx.transform.SetParent(null, true); // detach from module before despawn
            
            Destroy(vfx, lifetime);
        }

        private void ShowFloatingText(string text, Color color)
        {
            if (floatingTextPrefab == null) return;

            var txtObj = prefabPool.Spawn(floatingTextPrefab, transform.position, Quaternion.identity);
            txtObj.transform.SetParent(null, true);
            var textMesh = txtObj.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
            }
            
            txtObj.transform.DOMoveY(transform.position.y + 1f, 1f).OnComplete(() =>
            {
                prefabPool.Despawn(txtObj); 
            });
        }
    }
}
