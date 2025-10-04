using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships.Modules
{
    [RequireComponent(typeof(Collider2D))]
    public class ShipModule : MonoBehaviour
    {
        [SerializeField] private ModuleConfig moduleConfig;
        
        [Inject] private IPrefabPool prefabPool;
        
        private TrainController train;
        private int health = 1;

        public ModuleType Type => moduleConfig.ModuleType;
        public bool RequiresRepair => health < moduleConfig.MaxModuleHealth;
        public int Score => Mathf.RoundToInt(moduleConfig.Score * health / (float) moduleConfig.MaxModuleHealth);

        public void Repair(int repairValue = 0)
        {
            if (repairValue == 0)
            {
                health = moduleConfig.MaxModuleHealth;
                return;
            }
            
            health = Mathf.Clamp(health + repairValue, 0, moduleConfig.MaxModuleHealth);
        }

        private void Start()
        {
            health = moduleConfig.MaxModuleHealth;
        }

        public void AssignToTrain(TrainController t)
        {
            train = t;
        }

        public void DestroyCar()
        {
            if (train != null)
            {
                train.RemoveModule(this);
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