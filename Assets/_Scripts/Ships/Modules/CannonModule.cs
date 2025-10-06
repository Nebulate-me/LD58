using _Scripts.Utils.AudioTool.Sounds;
using Signals;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(ShipModule))]
    public class CannonModule : MonoBehaviour, IPoolableResource
    {
        [Header("References")]
        [SerializeField] private ShipModule shipModule;
        [SerializeField] private Transform turret;
        [SerializeField] private Transform firePoint;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject playerProjectilePrefab;
        [SerializeField] private GameObject enemyProjectilePrefab;

        [Header("Stats")]
        [SerializeField] private float fireCooldown = 0.6f;
        [SerializeField] private float projectileSpeed = 8f;

        [Inject] private IPrefabPool prefabPool;
        
        private float cooldownTimer;
        private Vector2 facing = Vector2.right;

        private bool IsPlayer => shipModule.Train && shipModule.Train.IsPlayerControlled;

        private void Awake() => shipModule = GetComponent<ShipModule>();
        
        public void OnSpawn()
        {
            
        }

        public void OnDespawn()
        {
            
        }

        private void Update()
        {
            if (shipModule.Train == null || !shipModule.Train.HasLocomotive)
                return;
            
            cooldownTimer -= Time.deltaTime;
            
            facing = IsPlayer ? Vector2.right : Vector2.left;
            float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            Quaternion turretRotation = Quaternion.Euler(0, 0, angle);
            turret.SetPositionAndRotation(turret.position, turretRotation);

            if (IsPlayer)
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
            if (cooldownTimer > 0f || firePoint == null) return;
            cooldownTimer = fireCooldown;

            var prefab = IsPlayer ? playerProjectilePrefab : enemyProjectilePrefab;
            var proj = prefabPool.Spawn(prefab, firePoint.position, Quaternion.Euler(0, 0,
                Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg));
            if (proj.TryGetComponent(out ProjectileController projectile))
            {
                SignalsHub.DispatchAsync(new PlaySoundSignal {Name = SoundName.CannonShot});
                projectile.Initialize(IsPlayer, projectileSpeed);
            }
        }
    }
}
