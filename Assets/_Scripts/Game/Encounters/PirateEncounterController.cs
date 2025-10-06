using System.Collections.Generic;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using _Scripts.Ships.ShipControllers;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game.Encounters
{
    public class PirateEncounterController : MonoBehaviour, IPoolableResource
    {
        [SerializeField] private GameObject pirateTrainPrefab;
        [SerializeField] private Vector3 baseSpawnPosition = new(6f, 0f, 0f);

        [Inject] private IPrefabPool pool;
        [Inject] private IGameFlowController gameFlowController;
        [Inject] private IModuleRegistry moduleRegistry;

        public void OnSpawn()
        {
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Pirate, out var locomotiveModuleConfig)) return;
            if (!moduleRegistry.TryGetModuleConfig(ModuleType.Turret, out var cannonModuleConfig)) return;
            if (!gameFlowController.TryGetPlayer(out var playerShip)) return;
            
            var pirateCount = Mathf.RoundToInt(Mathf.Lerp(1, 4, gameFlowController.CurrentDifficulty));
            for (var i = 0; i < pirateCount; i++)
            {
                var spawnPosition = baseSpawnPosition + new Vector3(0, i * 3f, 0);
                var trainObj = pool.Spawn(pirateTrainPrefab, spawnPosition, Quaternion.identity, transform);
                var train = trainObj.GetComponent<TrainController>();

                var modules = new List<ModuleConfig> {locomotiveModuleConfig};
                
                var turretModuleCount = Random.Range(1, 3);
                for (var c = 0; c < turretModuleCount; c++)
                {
                    modules.Add(cannonModuleConfig); 
                }
                
                var cargoModuleCount = Random.Range(1, 2);
                for (var c = 0; c < cargoModuleCount; c++)
                {
                    var cargoType = (CargoType)Random.Range((int)CargoType.Material, (int) CargoType.Contraband);
                    if (moduleRegistry.TryGetCargoModuleConfig(cargoType, out var cargoConfig))
                    {
                        modules.Add(cargoConfig);   
                    }
                }
                
                train.AssembleShip(new ShipConfiguration
                {
                    Facing = FacingDirection.Left,
                    Modules = new List<ModuleConfig> {locomotiveModuleConfig, cannonModuleConfig }
                }, spawnPosition);
                
                var pirateTrain = trainObj.GetComponent<PirateShipController>();
                pirateTrain.Initialize(playerShip.transform);
            }
        }

        public void OnDespawn() { }
    }
}