using System.Linq;
using _Scripts.Common;
using UnityEngine;
using _Scripts.Ships.Modules;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class PirateShipController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private float cargoPickupRadius = 4f;
        [SerializeField] private float attackAlignTolerance = 12f;
        [SerializeField] private float retreatHealthThreshold = 0.25f;
        [SerializeField] private float screenRightLimit = 8f;

        private TrainController _train;
        private Transform _player;
        private CannonModule[] _cannons;
        private IHealth _locomotiveHealth;

        private void Awake() => _train = GetComponent<TrainController>();

        public void Initialize(Transform player)
        {
            _player = player;
            _cannons = GetComponentsInChildren<CannonModule>();
            _locomotiveHealth = _train.GetModules().FirstOrDefault(m => m.Type == ModuleType.Locomotive)?.Health;
        }

        private void Update()
        {
            if (_player == null) return;

            // stop everything if train destroyed
            if (_locomotiveHealth == null || !_locomotiveHealth.IsAlive)
            {
                foreach (var c in _cannons) c.enabled = false;
                return;
            }

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
                // 2️⃣ Attack logic: align horizontally (left-facing)
                float angle = Vector2.Angle(Vector2.left, toPlayer);
                if (angle < attackAlignTolerance)
                {
                    foreach (var c in _cannons) c.SetFacing(Vector2.left);
                    moveDir = Vector2.left * 0.3f; // small drift left while attacking
                }
                else
                {
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

            // 4️⃣ Retreat when low HP
            if (_locomotiveHealth.CurrentHealth < _locomotiveHealth.MaxHealth * retreatHealthThreshold)
            {
                foreach (var c in _cannons) c.enabled = false; // stop firing
                moveDir = Vector2.left; // flee off-screen
            }

            // 5️⃣ Clamp to screen right edge
            if (pos.x > screenRightLimit)
                pos.x = screenRightLimit;

            transform.position = pos + moveDir * moveSpeed * Time.deltaTime;
        }
    }
}
