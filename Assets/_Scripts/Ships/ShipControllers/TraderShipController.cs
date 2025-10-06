using System.Linq;
using UnityEngine;
using _Scripts.Ships.Modules;
using Utilities.Prefabs;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class TraderShipController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private float cargoPickupRadius = 4f;

        private TrainController _train;
        private Transform _player;

        private void Awake() => _train = GetComponent<TrainController>();
        
        

        public void Initialize(Transform player) => _player = player;

        private void Update()
        {
            if (_player == null) return;

            Vector2 pos = transform.position;
            Vector2 moveDir = Vector2.left; // base drift

            // 1️⃣ Dodge bullets
            var incoming = FindObjectsOfType<ProjectileController>()
                .Where(p => p.IsPlayerProjectile &&
                            Vector2.Distance(p.transform.position, pos) < dodgeRadius);
            if (incoming.Any())
            {
                Vector2 avg = incoming.Aggregate(Vector2.zero, (a, p) => a + (Vector2)p.transform.right)
                              / incoming.Count();
                moveDir = Vector2.Perpendicular(avg).normalized;
            }

            // 2️⃣ Cargo pickup preference
            var loose = FindObjectsOfType<ShipModule>()
                .Where(m => m.Type == ModuleType.Cargo && m.Train == null)
                .OrderBy(m => Vector2.Distance(pos, m.transform.position))
                .FirstOrDefault();
            if (loose && Vector2.Distance(pos, loose.transform.position) < cargoPickupRadius)
                moveDir = (loose.transform.position - transform.position).normalized;

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}