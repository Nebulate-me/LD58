using _Scripts.Common;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(Collider2D), typeof(Health))]
    public class ShipModule : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Health health;
        [SerializeField] private ModuleConfig moduleConfig;
        
        [Inject] private IPrefabPool prefabPool;

        public ModuleType Type => moduleConfig.ModuleType;
        public bool RequiresRepair => health.CurrentHealth < moduleConfig.MaxModuleHealth;
        public int Score => Mathf.RoundToInt(moduleConfig.Score * health.CurrentHealth / (float) moduleConfig.MaxModuleHealth);
        public IHealth Health => health;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public TrainController Train { get; private set; }

        public void Repair(int repairValue = 0)
        {
            if (repairValue == 0)
            {
                health.Heal(health.MaxHealth);
                return;
            }
            
            health.Heal(repairValue);
        }

        private void Start()
        {
            health.SetUp(moduleConfig.MaxModuleHealth);
        }

        public void AssignToTrain(TrainController t)
        {
            Train = t;
        }

        public void DestroyModule()
        {
            if (Train != null)
            {
                Train.RemoveModule(this);
            }
            
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
    }
}