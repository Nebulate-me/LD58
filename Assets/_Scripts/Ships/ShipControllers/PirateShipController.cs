using System.Linq;
using _Scripts.Common;
using UnityEngine;
using _Scripts.Ships.Modules;
using _Scripts.Utils;

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
            if (_train.GetModules().TryGetFirst(module => module.Type == ModuleType.Locomotive, out var locomotiveModule))
            {
                _locomotiveHealth = locomotiveModule.Health;
            }
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
                            Vector2.Distance(p.transform.position, pos) < dodgeRadius)
                .ToList();
            if (incoming.Any())
            {
                Vector2 avg = incoming.Aggregate(Vector2.zero, (a, p) => a + (Vector2)p.transform.right)
                                 / incoming.Count();
                moveDir = Vector2.Perpendicular(avg).normalized;
            }
            else
            {
                // 2️⃣ Attack logic: close in horizontally (left-facing)
                float horizontalGap = pos.x - _player.position.x;

                if (horizontalGap > 3f)
                {
                    // move further left into screen until within 3 units of player x
                    moveDir = Vector2.left;
                }
                else if (horizontalGap < 1f)
                {
                    // too close, back slightly right to maintain spacing
                    moveDir = Vector2.right * 0.5f;
                }
                else
                {
                    // good range – align vertically for shooting window
                    moveDir = new Vector2(0, Mathf.Sign(toPlayer.y));
                }

                moveDir = CheckCargoPickup(pos, moveDir);
            }

            // 4️⃣ Retreat when low HP
            if (_locomotiveHealth != null && _locomotiveHealth.CurrentHealth < _locomotiveHealth.MaxHealth * retreatHealthThreshold)
            {
                foreach (var c in _cannons) c.enabled = false; // stop firing
                moveDir = Vector2.left; // flee off-screen
            }

            // 5️⃣ Clamp to screen right edge
            if (pos.x > screenRightLimit)
                pos.x = screenRightLimit;

            transform.position = pos + moveDir * moveSpeed * Time.deltaTime;
        }

        private Vector2 CheckCargoPickup(Vector2 pos, Vector2 moveDir)
        {
            var loose = FindObjectsOfType<ShipModule>()
                .Where(m => m.Type == ModuleType.Cargo && m.Train == null)
                .OrderBy(m => Vector2.Distance(pos, m.transform.position))
                .FirstOrDefault();
            if (loose && Vector2.Distance(pos, loose.transform.position) < cargoPickupRadius)
                moveDir = (loose.transform.position - transform.position).normalized;
            return moveDir;
        }
    }
}
