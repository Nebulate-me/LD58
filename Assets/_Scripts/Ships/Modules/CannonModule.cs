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
        [SerializeField] private Transform cannonPivot;   // pivot that rotates
        [SerializeField] private Transform firePoint;     // where projectiles spawn
        [SerializeField] private GameObject playerProjectilePrefab;
        [SerializeField] private GameObject enemyProjectilePrefab;

        [Header("Stats")]
        [SerializeField] private float rotationSpeed = 240f;
        [SerializeField] private float fireCooldown = 0.6f;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float detectionRadius = 12f;
        
        [Inject] private IPrefabPool prefabPool;

        private ShipModule _shipModule;
        private float _cooldownTimer;
        private Transform _currentTarget;

        private bool _isPlayer => _shipModule.Train != null && _shipModule.Train.IsPlayerControlled;

        private void Awake()
        {
            _shipModule = GetComponent<ShipModule>();
        }

        private void Update()
        {
            _cooldownTimer -= Time.deltaTime;

            AcquireTarget();
            RotateCannon();

            // Player fires manually; pirates auto-fire
            if (_isPlayer)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    TryFire();
            }
            else
            {
                if (_currentTarget != null)
                    TryFire();
            }
        }

        private void AcquireTarget()
        {
            Transform bestTarget = null;
            float bestDist = detectionRadius;

            // --- 1️⃣ Search for hostile ships first
            var modules = FindObjectsOfType<ShipModule>();
            foreach (var m in modules)
            {
                if (m.Train == null || m.Train == _shipModule.Train) continue; // same side
                if (!m.Health.IsAlive) continue;

                float dist = Vector2.Distance(transform.position, m.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = m.transform;
                }
            }

            // --- 2️⃣ If no ship target found, search for asteroids
            if (bestTarget == null)
            {
                var asteroids = FindObjectsOfType<AsteroidController>();
                foreach (var a in asteroids)
                {
                    if (a == null || !a.gameObject.activeInHierarchy) continue;
                    if (!a.Health.IsAlive) continue;

                    float dist = Vector2.Distance(transform.position, a.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestTarget = a.transform;
                    }
                }
            }

            _currentTarget = bestTarget;
        }

        private void RotateCannon()
        {
            if (_currentTarget == null || cannonPivot == null) return;

            Vector2 dir = _currentTarget.position - cannonPivot.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            cannonPivot.rotation = Quaternion.RotateTowards(
                cannonPivot.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        private void TryFire()
        {
            if (_cooldownTimer > 0f || firePoint == null) return;
            _cooldownTimer = fireCooldown;

            GameObject prefab = _isPlayer ? playerProjectilePrefab : enemyProjectilePrefab;
            if (prefab == null) return;

            var proj = prefabPool.Spawn(prefab, firePoint.position, firePoint.rotation);
            if (proj.TryGetComponent(out ProjectileController projectile))
            {
                projectile.Initialize(_isPlayer, projectileSpeed);
            }
        }
    }
}
