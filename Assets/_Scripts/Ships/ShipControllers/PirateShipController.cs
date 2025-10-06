using System.Linq;
using _Scripts.Common;
using _Scripts.Game;
using UnityEngine;
using _Scripts.Ships.Modules;
using _Scripts.Utils;
using Sirenix.OdinInspector;
using Utilities.Prefabs;
using Zenject;

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
        [SerializeField] private float retreatHealthThreshold = 0.25f;
        [SerializeField] private float screenRightLimit = 8f;
        
        [Inject] private IGameFlowController gameFlowController;

        [ShowInInspector,ReadOnly] private TrainController pirateShip;
        [ShowInInspector,ReadOnly] private TrainController playerShip;
        
        public void OnSpawn()
        {
            pirateShip = GetComponent<TrainController>();
            _ = gameFlowController.TryGetPlayer(out playerShip);
        }

        public void OnDespawn()
        {
        }

        private void Update()
        {
            if (playerShip == null) return;

            Vector2 headPosition = pirateShip.Head.position;
            Vector2 playerHeadDirection = (Vector2)playerShip.Head.position - headPosition;
            Vector2 moveDirection = Vector2.zero;

            // 1️⃣ Dodge nearby projectiles
            var incoming = FindObjectsOfType<ProjectileController>()
                .Where(p => p.IsPlayerProjectile &&
                            Vector2.Distance(p.transform.position, headPosition) < dodgeRadius)
                .ToList();
            if (incoming.Any())
            {
                Vector2 avg = incoming.Aggregate(Vector2.zero, (a, p) => a + (Vector2)p.transform.right)
                                 / incoming.Count();
                moveDirection = Vector2.Perpendicular(avg).normalized;
            }
            else
            {
                // 2️⃣ Attack logic: close in horizontally (left-facing)
                float horizontalGap = headPosition.x - playerShip.Head.position.x;

                if (horizontalGap > 3f)
                {
                    // move further left into screen until within 3 units of player x
                    moveDirection = Vector2.left;
                }
                else if (horizontalGap < 1f)
                {
                    // too close, back slightly right to maintain spacing
                    moveDirection = Vector2.right * 0.5f;
                }
                else
                {
                    // good range – align vertically for shooting window
                    moveDirection = new Vector2(0, Mathf.Sign(playerHeadDirection.y));
                }

                moveDirection = CheckCargoPickup(headPosition, moveDirection);
            }

            // 5️⃣ Clamp to screen right edge
            if (headPosition.x > screenRightLimit)
                headPosition.x = screenRightLimit;

            transform.position = headPosition + moveDirection * moveSpeed * Time.deltaTime;
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
