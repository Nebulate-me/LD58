using System.Linq;
using UnityEngine;
using _Scripts.Ships.Modules;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class TraderShipController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private float cargoPickupRadius = 4f;
        [SerializeField] private float donationDistance = 6f;

        private TrainController train;
        private Transform player;

        private void Awake()
        {
            train = GetComponent<TrainController>();
        }

        public void Initialize(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void Update()
        {
            if (player == null) return;

            Vector2 pos = transform.position;
            Vector2 moveDir = Vector2.left; // base drift (move off screen)

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

            // 2️⃣ Cargo pickup
            var loose = FindObjectsOfType<ShipModule>()
                .Where(m => m.Type == ModuleType.Cargo && m.Train == null)
                .OrderBy(m => Vector2.Distance(pos, m.transform.position))
                .FirstOrDefault();
            if (loose && Vector2.Distance(pos, loose.transform.position) < cargoPickupRadius)
                moveDir = (loose.transform.position - transform.position).normalized;

            // 3️⃣ Cargo donation to player
            float distToPlayer = Vector2.Distance(player.position, pos);
            if (distToPlayer < donationDistance)
            {
                // give last cargo
                var cargo = train.GetModules()
                    .LastOrDefault(m => m.Type == ModuleType.Cargo);
                if (cargo != null)
                {
                    train.RemoveModule(cargo);
                    cargo.Detach();

                    // gently move it toward player
                    cargo.StartCoroutine(MoveCargoToPlayer(cargo.transform, player));

                    Debug.Log($"🤝 Trader {name} donated cargo {cargo.name} to player.");
                }
            }

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
        }

        private System.Collections.IEnumerator MoveCargoToPlayer(Transform cargo, Transform player)
        {
            float t = 0f;
            Vector3 start = cargo.position;
            while (t < 1f && cargo != null && player != null)
            {
                t += Time.deltaTime;
                cargo.position = Vector3.Lerp(start, player.position, t);
                yield return null;
            }
        }
    }
}
