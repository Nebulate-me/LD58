using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Common.Boundary
{
    /// <summary>
    /// Destroys the GameObject when it fully leaves the visible screen bounds.
    /// Ideal for projectiles, asteroids, traders, etc.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class ScreenBoundaryDespawner : MonoBehaviour, IPoolableResource
    {
        private Camera cam;
        private Renderer _renderer;
        
        [Inject] private IPrefabPool prefabPool;
        [Inject] private ICommonSettingsProvider commonSettingsProvider;
        
        public void OnSpawn()
        {
            cam = Camera.main;
            _renderer = GetComponent<Renderer>();
        }

        public void OnDespawn()
        {
         
        }

        private void Update()
        {
            if (cam == null || _renderer == null) return;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float leftEdge = cam.transform.position.x - halfWidth - commonSettingsProvider.OtherMargins;
            float rightEdge = cam.transform.position.x + halfWidth + commonSettingsProvider.RightMargin;
            float topEdge = cam.transform.position.y + halfHeight + commonSettingsProvider.OtherMargins;
            float bottomEdge = cam.transform.position.y - halfHeight - commonSettingsProvider.OtherMargins;

            Bounds bounds = _renderer.bounds;

            if (bounds.max.x < leftEdge ||
                bounds.min.x > rightEdge ||
                bounds.min.y > topEdge ||
                bounds.max.y < bottomEdge)
            {
                prefabPool.Despawn(gameObject);
            }
        }
    }
}