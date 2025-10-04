using _Scripts.Ships;
using _Scripts.Ships.Modules;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Utilities.Prefabs;

namespace _Scripts.Game
{
    public class GameInitializer : MonoBehaviour, IGameInitializer
    {
        [Header("Parents")]
        [FormerlySerializedAs("trainParent")]
        [SerializeField] private Transform shipParent;
        [SerializeField] private Transform staticParent;

        [Header("Trains")]
        [SerializeField] private GameObject playerTrainPrefab;
        
        [Header("Buildings")]
        [SerializeField] private GameObject taxStationPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 playerSpawnPos = new(0, 0);
        [SerializeField] private Vector2 aiSpawnPos = new(10, 0);

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IModuleRegistry moduleRegistry;
        
        public TrainController PlayerTrain { get; private set; }
        public Vector2 PlayerPosition => PlayerTrain != null 
            ? PlayerTrain.transform.position
            : new Vector2(playerSpawnPos.x, playerSpawnPos.y);

        private void Start()
        {
            PlayerTrain = SpawnTrain(playerTrainPrefab, isPlayer: true, playerSpawnPos);
        }
        
        public TrainController SpawnTrain(GameObject trainPrefab, bool isPlayer, Vector2 spawnPos)
        {
            // 1️⃣ Spawn base train object
            var trainObj = prefabPool.Spawn(trainPrefab, spawnPos, Quaternion.identity, shipParent);
            var controller = trainObj.GetComponent<TrainController>();

            if (moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Player, out var locomotiveModule))
            {
                var loco = prefabPool.Spawn(locomotiveModule.Prefab, trainObj.transform);
                controller.AddModule(loco.GetComponent<ShipModule>());   
            }


            if (moduleRegistry.TryGetCargoModuleConfig(CargoType.Material, out var cargoModule))
            {
                var cargo = prefabPool.Spawn(cargoModule.Prefab, trainObj.transform);
                controller.AddModule(cargo.GetComponent<ShipModule>());
            }

            // 4️⃣ Player-specific logic
            if (isPlayer)
            {
                var playerController = trainObj.GetComponent<PlayerTrainController>();
                playerController.SetPlayerControl(true);
            }

            return controller;
        }
    }
}