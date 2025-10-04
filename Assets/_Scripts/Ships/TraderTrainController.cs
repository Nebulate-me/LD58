using System.Collections;
using _Scripts.Ships.Modules;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships
{
    [RequireComponent(typeof(TrainController))]
    public class TraderTrainController : MonoBehaviour
    {
        [Header("Trader Setup")]
        [SerializeField] private int minCargoCount = 1;
        [SerializeField] private int maxCargoCount = 3;

        [Header("AI Behaviour")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float transferDelay = 1.5f;
        [SerializeField] private float fleeSpeedMultiplier = 1.5f;
        [SerializeField] private float despawnDistance = 40f;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IModuleRegistry moduleRegistry;

        private TrainController _train;
        private Transform _player;
        private bool _hasTransferred;
        private Vector2 _moveDir;
        private Coroutine _transferRoutine;

        public void SetUp(Transform player)
        {
            _player = player;
            _train = GetComponent<TrainController>();
            _moveDir = Random.value > 0.5f ? Vector2.left : Vector2.right;
            
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Trader, out var locomotiveConfig)) return;
            
            var locomotiveObject = prefabPool.Spawn(locomotiveConfig.Prefab, transform);
            _train.AddModule(locomotiveObject.GetComponent<ShipModule>());
            
            var cargoCount = Random.Range(minCargoCount, maxCargoCount + 1);
            for (var i = 0; i < cargoCount; i++)
            {
                // random cargo type from enum (excluding None)
                var randomCargoType = (CargoType)Random.Range(1, System.Enum.GetValues(typeof(CargoType)).Length);
                if (!moduleRegistry.TryGetCargoModuleConfig(randomCargoType, out var cargoConfig)) return;
                
                var cargo = prefabPool.Spawn(cargoConfig.Prefab, transform);
                var module = cargo.GetComponent<ShipModule>();

                _train.AddModule(module);
            }

            Debug.Log($"Trader spawned with {cargoCount} cargo modules.");
        }

        private void Update()
        {
            if (_player == null || _train == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (!_hasTransferred && distanceToPlayer < detectionRadius)
            {
                if (_transferRoutine == null)
                    _transferRoutine = StartCoroutine(PerformTransfer());
            }

            // Move away or continue normal travel
            var dir = ((Vector2) (_hasTransferred ? (transform.position - _player.position) : _moveDir)).normalized;
            transform.position += (Vector3)(dir * moveSpeed * (_hasTransferred ? fleeSpeedMultiplier : 1f) * Time.deltaTime);
            
            if (Vector2.Distance(transform.position, _player.position) > despawnDistance)
                prefabPool.Despawn(gameObject);
        }

        private IEnumerator PerformTransfer()
        {
            Debug.Log("Trader stopped for taxation...");
            float timer = 0f;
            while (timer < transferDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            TransferCargoToPlayer();
            _hasTransferred = true;
            Debug.Log("Trader resumes journey after taxation.");
        }

        private void TransferCargoToPlayer()
        {
            if (_train.GetModules().Count <= 1) return;

            var cargoToTransfer = _train.GetModules()[_train.GetModules().Count - 1];
            var playerTrain = _player.GetComponentInChildren<TrainController>();
            if (playerTrain == null) return;

            _train.RemoveModule(cargoToTransfer);
            playerTrain.AddModule(cargoToTransfer);

            Debug.Log($"📦 Trader transferred cargo ({cargoToTransfer.Type}) to player!");
        }
    }
}
