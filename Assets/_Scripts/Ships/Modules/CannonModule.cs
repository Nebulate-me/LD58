using UnityEngine;
using _Scripts.Static;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(ShipModule))]
    public class CannonModule : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject playerProjectilePrefab;
        [SerializeField] private GameObject enemyProjectilePrefab;

        [Header("Stats")]
        [SerializeField] private float fireCooldown = 0.6f;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private bool eightDirectional = false;

        [Inject] private IPrefabPool prefabPool;

        private ShipModule _shipModule;
        private float _cooldownTimer;
        private Vector2 _facing = Vector2.right;

        private bool _isPlayer => _shipModule.Train && _shipModule.Train.IsPlayerControlled;

        private void Awake() => _shipModule = GetComponent<ShipModule>();

        public void SetFacing(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.01f) return;
            _facing = eightDirectional ? SnapToEight(dir) : dir.normalized;
            float z = Mathf.Atan2(_facing.y, _facing.x) * Mathf.Rad2Deg;
            firePoint.localRotation = Quaternion.Euler(0, 0, z);
        }

        private void Update()
        {
            _cooldownTimer -= Time.deltaTime;

            if (_isPlayer)
            {
                if (Input.GetKey(KeyCode.Space))
                    TryFire();
            }
            else
            {
                TryFire();
            }
        }

        private void TryFire()
        {
            if (_cooldownTimer > 0f || firePoint == null) return;
            _cooldownTimer = fireCooldown;

            var prefab = _isPlayer ? playerProjectilePrefab : enemyProjectilePrefab;
            var proj = prefabPool.Spawn(prefab, firePoint.position, Quaternion.Euler(0, 0,
                Mathf.Atan2(_facing.y, _facing.x) * Mathf.Rad2Deg));
            if (proj.TryGetComponent(out ProjectileController projectile))
                projectile.Initialize(_isPlayer, projectileSpeed);
        }

        private static Vector2 SnapToEight(Vector2 dir)
        {
            Vector2[] dirs = {
                Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                new Vector2(1,1).normalized, new Vector2(-1,1).normalized, new Vector2(1,-1).normalized, new Vector2(-1,-1).normalized
            };
            float best = 999f; Vector2 bestDir = Vector2.right;
            foreach (var d in dirs)
            {
                float a = Vector2.Angle(d, dir);
                if (a < best) { best = a; bestDir = d; }
            }
            return bestDir;
        }
    }
}
