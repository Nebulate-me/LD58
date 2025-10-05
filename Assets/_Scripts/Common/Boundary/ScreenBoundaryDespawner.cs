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
    public class ScreenBoundaryDespawner : MonoBehaviour
    {
        private Camera _cam;
        private Renderer _renderer;
        
        [Inject] private IPrefabPool prefabPool;
        [Inject] private ICommonSettingsProvider commonSettingsProvider;

        private void Awake()
        {
            _cam = Camera.main;
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_cam == null || _renderer == null) return;

            Vector3 pos = transform.position;

            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;

            float leftEdge = _cam.transform.position.x - halfWidth - commonSettingsProvider.OtherMargins;
            float rightEdge = _cam.transform.position.x + halfWidth + commonSettingsProvider.RightMargin;
            float topEdge = _cam.transform.position.y + halfHeight + commonSettingsProvider.OtherMargins;
            float bottomEdge = _cam.transform.position.y - halfHeight - commonSettingsProvider.OtherMargins;

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