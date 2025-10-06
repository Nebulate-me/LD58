using System.Collections.Generic;
using System.Numerics;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using _Scripts.Utils.AudioTool.Music;
using _Scripts.Utils.AudioTool.Music.Signals;
using Signals;
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

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 playerSpawnPos = new(0, 0, 0);
        [SerializeField] private float carSpacing = 1.5f;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IModuleRegistry moduleRegistry;
        
        public TrainController PlayerShip { get; private set; }
        public Vector2 PlayerPosition => PlayerShip != null 
            ? PlayerShip.Head.transform.position
            : new Vector2(playerSpawnPos.x, playerSpawnPos.y);

        private void Start()
        {
            SignalsHub.DispatchAsync(new PlayMusicSignal {Type = MusicType.MenuMusic});
            SpawnPlayerTrain(playerTrainPrefab);
        }

        private void SpawnPlayerTrain(GameObject trainPrefab)
        {
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Player, out var locomotiveConfig)) return;
            if (!moduleRegistry.TryGetCargoModuleConfig(CargoType.Material, out var cargoConfig)) return;
            if (!moduleRegistry.TryGetModuleConfig(ModuleType.Turret, out var cannonConfig)) return;
            
            PlayerShip = prefabPool.Spawn(trainPrefab, playerSpawnPos, Quaternion.identity).GetComponent<TrainController>();
            PlayerShip.IsPlayerControlled = true;
            var startingPlayerShipConfiguration = new ShipConfiguration
            {
                Facing = FacingDirection.Right,
                Modules = new List<ModuleConfig> { locomotiveConfig, cargoConfig, cannonConfig}
            };
            PlayerShip.AssembleShip(startingPlayerShipConfiguration, playerSpawnPos);
        }

    }
}