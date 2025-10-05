using System.Numerics;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Utilities.Prefabs;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
        [SerializeField] private Vector3 playerSpawnPos = new(0, 0, 0);
        [SerializeField] private float carSpacing = 1.5f;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IModuleRegistry moduleRegistry;
        
        public TrainController PlayerShip { get; private set; }
        public Vector2 PlayerPosition => PlayerShip != null 
            ? PlayerShip.transform.position
            : new Vector2(playerSpawnPos.x, playerSpawnPos.y);

        private void Start()
        {
            SpawnPlayerTrain(playerTrainPrefab);
        }

        private void SpawnPlayerTrain(GameObject trainPrefab)
        {
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Player, out var locomotiveConfig)) return;
            if (!moduleRegistry.TryGetCargoModuleConfig(CargoType.Material, out var cargoConfig)) return;
            if (!moduleRegistry.TryGetModuleConfig(ModuleType.Turret, out var cannonConfig)) return;
            
            PlayerShip = prefabPool.Spawn(trainPrefab, playerSpawnPos, Quaternion.identity).GetComponent<TrainController>();
            PlayerShip.IsPlayerControlled = true;

            var cannon = prefabPool.Spawn(cannonConfig.Prefab, playerSpawnPos, Quaternion.identity, PlayerShip.transform);
            var cargo  = prefabPool.Spawn(cargoConfig.Prefab,  playerSpawnPos + Vector3.right * carSpacing, Quaternion.identity, PlayerShip.transform);
            var loco   = prefabPool.Spawn(locomotiveConfig.Prefab, playerSpawnPos + Vector3.right * carSpacing * 2f, Quaternion.identity, PlayerShip.transform);

            PlayerShip.AddModule(loco.GetComponent<ShipModule>());
            PlayerShip.AddModule(cargo.GetComponent<ShipModule>());
            PlayerShip.AddModule(cannon.GetComponent<ShipModule>());
        }

    }
}