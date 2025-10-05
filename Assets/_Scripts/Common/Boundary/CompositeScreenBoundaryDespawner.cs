using _Scripts.Ships;
using ModestTree;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Common.Boundary
{
    [RequireComponent(typeof(TrainController))]
    public class CompositeScreenBoundaryDespawner : MonoBehaviour
    {
        [SerializeField] private TrainController trainController;
        
        private Camera mainCamera;
        
        [Inject] private ICommonSettingsProvider commonSettingsProvider;
        [Inject] private IPrefabPool prefabPool;

        private void Awake()
        {
            mainCamera = Camera.main;
            
            if (trainController == null)
                trainController = GetComponent<TrainController>();
        }

        private void Update()
        {
            if (mainCamera == null) return;

            var childRenderers = trainController.GetModuleSpriteRenderers();
            if (childRenderers.IsEmpty()) return;

            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;

            float leftEdge = mainCamera.transform.position.x - halfWidth - commonSettingsProvider.OtherMargins;
            float rightEdge = mainCamera.transform.position.x + halfWidth + commonSettingsProvider.RightMargin;
            float topEdge = mainCamera.transform.position.y + halfHeight + commonSettingsProvider.OtherMargins;
            float bottomEdge = mainCamera.transform.position.y - halfHeight - commonSettingsProvider.OtherMargins;
            
            bool allOffScreen = true;
            foreach (var r in childRenderers)
            {
                if (r == null) continue;

                Bounds b = r.bounds;

                // if any part of any module is still visible/in-range, keep the train alive
                if (!(b.max.x < leftEdge || b.min.x > rightEdge || b.min.y > topEdge || b.max.y < bottomEdge))
                {
                    allOffScreen = false;
                    break;
                }
            }

            if (allOffScreen)
            {
                prefabPool.Despawn(gameObject);
            }
        }
    }
}
