using _Scripts.Common.Boundary;
using _Scripts.Ships.Modules;
using UnityEngine;

namespace _Scripts.Common
{
    [RequireComponent(typeof(ShipModule))]
    [RequireComponent(typeof(Health))]
    public class DetachableLoot : MonoBehaviour
    {
        private Health _health;
        private ShipModule _module;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _module = GetComponent<ShipModule>();
            _health.OnDeath += OnDestroyed;
        }

        private void OnDestroyed(Health _)
        {
            // detach from train so the module stops following
            if (_module.Train != null)
            {
                _module.Train.RemoveModule(_module);
            }

            transform.SetParent(null, true);

            // make sure it can float and persist
            if (!TryGetComponent(out Rigidbody2D rb))
                rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.left * 1f;

            // ensure ScreenBoundaryDespawner or CompositeBoundaryDespawner handles cleanup
            if (GetComponent<ScreenBoundaryDespawner>() == null)
            {
                gameObject.AddComponent<ScreenBoundaryDespawner>();
            }

            Debug.Log($"💎 Loot dropped: {name}");
        }
    }
}