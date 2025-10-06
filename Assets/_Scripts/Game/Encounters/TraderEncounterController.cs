using System.Collections.Generic;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using _Scripts.Ships.ShipControllers;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game.Encounters
{
    public class TraderEncounterController : MonoBehaviour, IPoolableResource
    {
        [SerializeField] private GameObject traderTrainPrefab;

        [Inject] private IPrefabPool pool;
        [Inject] private IGameFlowController gameFlow;
        [Inject] private IModuleRegistry moduleRegistry;

        private readonly List<GameObject> traders = new();

        public void OnSpawn()
        {
            if (!moduleRegistry.TryGetLocomotiveModuleConfig(LocomotiveType.Trader, out var locoConfig)) return;
            if (!gameFlow.TryGetPlayer(out var playerTrain)) return;

            // spawn 2 traders on opposite sides
            bool firstLeft = Random.value > 0.5f;
            for (int i = 0; i < 2; i++)
            {
                float side = (i == 0 ? (firstLeft ? -1 : 1) : (firstLeft ? 1 : -1));
                Vector3 pos = playerTrain.transform.position +
                              new Vector3(10f * side, Random.Range(-4f, 4f), 0);
                var traderObj = pool.Spawn(traderTrainPrefab, pos, Quaternion.identity, transform);
                var train = traderObj.GetComponent<TrainController>();
                var traderAI = traderObj.GetComponent<TraderShipController>();
                traderAI.Initialize(playerTrain.transform);

                var modules = new List<ModuleConfig> {locoConfig};

                // random 1–3 cargo cars
                var cargoModuleCount = Random.Range(1, 4);
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
                    Modules = modules
                }, pos);

                traders.Add(traderObj);
            }

            Debug.Log("🚂 Trader Encounter: two traders spawned.");
        }

        public void OnDespawn()
        {
            foreach (var t in traders)
                if (t != null) pool.Despawn(t);
            traders.Clear();
        }
    }
}
