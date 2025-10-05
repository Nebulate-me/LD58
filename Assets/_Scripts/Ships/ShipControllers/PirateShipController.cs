using System.Linq;
using UnityEngine;
using _Scripts.Ships.Modules;
using Utilities.Prefabs;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class PirateShipController : MonoBehaviour, IPoolableResource
    {
        [Header("AI Settings")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private float cargoPickupRadius = 4f;
        [SerializeField] private float attackAlignTolerance = 12f;

        private TrainController _train;
        private Transform _player;
        private CannonModule[] _cannons;

        private void Awake() => _train = GetComponent<TrainController>();

        public void Initialize(Transform player)
        {
            _player = player;
            _cannons = GetComponentsInChildren<CannonModule>();
        }

        private void Update()
        {
            if (_player == null) return;

            Vector2 pos = transform.position;
            Vector2 toPlayer = (Vector2)_player.position - pos;
            Vector2 moveDir = Vector2.zero;

            // 1️⃣ Dodge nearby projectiles
            var incoming = FindObjectsOfType<ProjectileController>()
                .Where(p => p.IsPlayerProjectile &&
                            Vector2.Distance(p.transform.position, pos) < dodgeRadius);
            if (incoming.Any())
            {
                Vector2 avg = incoming.Aggregate(Vector2.zero, (a, p) => a + (Vector2)p.transform.right)
                                 / incoming.Count();
                moveDir = Vector2.Perpendicular(avg).normalized;
            }
            else
            {
                // 2️⃣ Try to align horizontally with player for attack
                float angle = Vector2.Angle(Vector2.right, toPlayer);
                if (angle < attackAlignTolerance)
                {
                    foreach (var c in _cannons) c.SetFacing(Vector2.right);
                    moveDir = Vector2.right * 0.5f;
                }
                else
                {
                    // Adjust vertically to line up shot
                    moveDir = new Vector2(0, Mathf.Sign(toPlayer.y));
                }

                // 3️⃣ Cargo pickup
                var loose = FindObjectsOfType<ShipModule>()
                    .Where(m => m.Type == ModuleType.Cargo && m.Train == null)
                    .OrderBy(m => Vector2.Distance(pos, m.transform.position))
                    .FirstOrDefault();
                if (loose && Vector2.Distance(pos, loose.transform.position) < cargoPickupRadius)
                    moveDir = (loose.transform.position - transform.position).normalized;
            }

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }

        public void OnSpawn()
        {
            foreach (var mod in _train.GetModules())
            {
                mod.SetFacing(FacingDirection.Left);
            }
        }

        public void OnDespawn()
        {
           
        }
    }
}
