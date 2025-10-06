using System.Linq;
using _Scripts.Game;
using UnityEngine;
using _Scripts.Ships.Modules;
using _Scripts.Utils.AudioTool.Sounds;
using Signals;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class TraderShipController : MonoBehaviour, IPoolableResource
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private float cargoPickupRadius = 4f;
        [SerializeField] private float taxationDistance = 6f;

        [Inject] private IGameFlowController gameFlowController;
        
        private TrainController traderShip;
        private TrainController playerShip;
        private bool hasBeenTaxed = false;
        
        public void OnSpawn()
        {
            hasBeenTaxed = false;
            _ = gameFlowController.TryGetPlayer(out playerShip);
        }

        public void OnDespawn()
        {
        }

        private void Awake()
        {
            traderShip = GetComponent<TrainController>();
        }

        private void Update()
        {
            if (!playerShip) return;

            Vector2 headPosition = traderShip.Head.position;
            Vector2 moveDir = Vector2.left; // base drift (move off screen)

            // 1️⃣ Dodge bullets
            var incoming = FindObjectsOfType<ProjectileController>()
                .Where(p => p.IsPlayerProjectile &&
                            Vector2.Distance(p.transform.position, headPosition) < dodgeRadius)
                .ToList();
            if (incoming.Any())
            {
                Vector2 avg = incoming.Aggregate(Vector2.zero, (a, p) => a + (Vector2)p.transform.right)
                              / incoming.Count();
                moveDir = Vector2.Perpendicular(avg).normalized;
            }
            
            moveDir = CheckCargoPickup(headPosition, moveDir);

            CheckTax(headPosition);

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
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

        private void CheckTax(Vector2 pos)
        {
            if (hasBeenTaxed) return;
            
            var distToPlayer = Vector2.Distance(playerShip.Head.position, pos);
            if (!(distToPlayer < taxationDistance)) return;
            
            var cargo = traderShip.GetModules()
                .LastOrDefault(m => m.Type == ModuleType.Cargo);
            if (cargo == null) return;
                
            cargo.DetachFromShip();
            cargo.AttachToShip(playerShip);
            cargo.FlipFacing();
            hasBeenTaxed = true;
            SignalsHub.DispatchAsync(new PlaySoundSignal { Name = SoundName.Tax });
            Debug.Log($"🤝 Trader {name} taxed cargo {cargo.name} to player.");
        }
    }
}
