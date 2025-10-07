using System;
using System.Collections;
using _Scripts.Common;
using _Scripts.Utils.AudioTool.Sounds;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.Modules
{
    public enum FacingDirection
    {
        Left = -1, 
        Right = 1
    }

    [RequireComponent(typeof(Collider2D), typeof(Health))]
    public class ShipModule : MonoBehaviour, IPoolableResource
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Health health;
        [SerializeField] private ModuleConfig moduleConfig;
        [SerializeField] private FacingDirection facing = FacingDirection.Right;
        
        private SpriteRenderer[] sprites;
        
        [Inject] private IPrefabPool prefabPool;

        public ModuleType Type => moduleConfig.ModuleType;
        public bool RequiresRepair => health.CurrentHealth < moduleConfig.MaxModuleHealth;
        public int Score => moduleConfig.Score;
        public IHealth Health => health;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        [ShowInInspector,ReadOnly] public TrainController Ship { get; private set; }
        
        public void SetFacing(FacingDirection dir)
        {
            facing = dir;
            ApplyFacing();
        }
        
        public void FlipFacing()
        {
            facing = facing == FacingDirection.Left ? FacingDirection.Right : FacingDirection.Left;
            ApplyFacing();
        }

        private void ApplyFacing()
        {
            float sign = (float)facing;
            foreach (var sr in sprites)
            {
                var scale = sr.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * sign;
                sr.transform.localScale = scale;
            }
        }

        public void Repair(int repairValue = 0)
        {
            if (repairValue == 0)
            {
                health.Heal(health.MaxHealth);
                return;
            }
            
            health.Heal(repairValue);
        }
        
        private void Awake()
        {
            sprites = GetComponentsInChildren<SpriteRenderer>(true);
            ApplyFacing();
        }

        private void Start()
        {
            health.SetUp(moduleConfig.MaxModuleHealth);
        }

        public void AttachToShip(TrainController ship)
        {
            if (Ship != null) return;
            
            transform.SetParent(ship.transform);
            Ship = ship;
            Ship.AddModule(this);
            SetFacing(ship.Facing);
        }
        
        public void DetachFromShip()
        {
            if (Ship == null) return;
            
            Ship.RemoveModule(this);
            transform.SetParent(null);
            Ship = null;
        }

        private IEnumerator DelayedDespawn(float delay)
        {
            yield return new WaitForSeconds(delay);

            prefabPool.Despawn(gameObject);
        }

        public void UpdateHead(bool isPlayer)
        {
            if (!isPlayer) return;

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(moveX, moveY, 0f).normalized;
            transform.position += move * Time.deltaTime * 5f;
        }

        public void Follow(Vector3 targetPos, float spacing, float moveSpeed)
        {
            Vector3 dir = (targetPos - transform.position);
            float dist = dir.magnitude;

            if (dist > spacing)
            {
                Vector3 newPos = targetPos - dir.normalized * spacing;
                transform.position = Vector3.MoveTowards(transform.position, newPos, moveSpeed * Time.deltaTime);
            }
        }

        public void Sell(int pointsEarned)
        {
            SignalsHub.DispatchAsync(new PlaySoundSignal {Name = SoundName.Cash});
            if (TryGetComponent(out ModuleVFXController vfx))
                vfx.OnSold(pointsEarned);
            
            DetachFromShip();
            prefabPool.Despawn(gameObject);
        }

        public void OnSpawn()
        {
            health.OnDeath += OnDeath;
        }

        public void OnDespawn()
        {
            health.OnDeath -= OnDeath;
        }

        private void OnDeath(Health diedHealth)
        {
            DetachFromShip();
            prefabPool.Despawn(gameObject);
        }
    }
}