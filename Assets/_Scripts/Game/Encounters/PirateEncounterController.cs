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

        [Inject] private IPrefabPool pool;
        [Inject] private IGameFlowController gameFlowController;
        [Inject] private IModuleRegistry moduleRegistry;

        public void OnSpawn()
        {
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Pirate, out var locomotiveModuleConfig)) return;
            if (!moduleRegistry.TryGetModuleConfig(ModuleType.Turret, out var cannonModuleConfig)) return;
            if (!gameFlowController.TryGetPlayer(out var playerShip)) return;
            
            int pirates = Mathf.RoundToInt(Mathf.Lerp(1, 3, gameFlowController.CurrentDifficulty));
            for (var i = 0; i < pirates; i++)
            {
                var trainObj = pool.Spawn(pirateTrainPrefab, transform.position + new Vector3(0, i * 3f, 0), Quaternion.identity, transform);
                var train = trainObj.GetComponent<TrainController>();
                var pirateTrain = trainObj.GetComponent<PirateShipController>();
                pirateTrain.Initialize(playerShip.transform);
                
                var locomotive = pool.Spawn(locomotiveModuleConfig.Prefab, trainObj.transform);
                train.AddModule(locomotive.GetComponent<ShipModule>());
                
                var cannon = pool.Spawn(cannonModuleConfig.Prefab, trainObj.transform);
                train.AddModule(cannon.GetComponent<ShipModule>());
            }
        }

        public void OnDespawn() { }
    }
}