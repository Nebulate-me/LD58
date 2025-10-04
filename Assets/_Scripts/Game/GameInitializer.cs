using _Scripts.Ships;
using _Scripts.Ships.Modules;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Utilities.Prefabs;

namespace _Scripts.Game
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Parents")]
        [SerializeField] private Transform trainParent;
        [SerializeField] private Transform staticParent;

        [Header("Trains")]
        [SerializeField] private GameObject playerTrainPrefab;
        [SerializeField] private GameObject traderTrainPrefab;
        
        [Header("Modules")]
        [SerializeField] private GameObject locomotiveModulePrefab;
        [SerializeField] private GameObject cargoModulePrefab;
        
        [Header("Buildings")]
        [SerializeField] private GameObject taxStationPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 playerSpawnPos = new(0, 0);
        [SerializeField] private Vector2 aiSpawnPos = new(10, 0);

        [Inject] private IPrefabPool prefabPool;

        private void Start()
        {
            SpawnTrain(playerTrainPrefab, isPlayer: true, playerSpawnPos);
            SpawnTrain(traderTrainPrefab, isPlayer: false, aiSpawnPos);
        }
        
        private TrainController SpawnTrain(GameObject trainPrefab, bool isPlayer, Vector2 spawnPos)
        {
            // 1️⃣ Spawn base train object
            var trainObj = prefabPool.Spawn(trainPrefab, spawnPos, Quaternion.identity, trainParent);
            var controller = trainObj.GetComponent<TrainController>();

            // 2️⃣ Spawn locomotive
            var loco = prefabPool.Spawn(locomotiveModulePrefab, trainObj.transform);
            controller.AddModule(loco.GetComponent<ShipModule>());

            // 3️⃣ Spawn cargo
            var cargo = prefabPool.Spawn(cargoModulePrefab, trainObj.transform);
            controller.AddModule(cargo.GetComponent<ShipModule>());

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