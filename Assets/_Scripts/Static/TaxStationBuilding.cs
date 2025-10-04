using System.Collections.Generic;
using System.Linq;
using _Scripts.Game;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using UnityEngine;
using Zenject;

namespace _Scripts.Static
{
    [RequireComponent(typeof(Collider2D))]
    public class TaxStationBuilding : MonoBehaviour
    {
        [Header("Station Settings")]
        [SerializeField] private int scorePerCargo = 100;
        [SerializeField] private bool destroyAfterUse = false;
        
        [Inject] private IScoreService scoreService;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var trainController = other.GetComponentInParent<TrainController>();
            if (trainController == null) return;

            if (!trainController.IsPlayerControlled) return;

            HandlePlayerDock(trainController);
        }

        private void HandlePlayerDock(TrainController train)
        {
            var modulesToDestroy = new List<ShipModule>();
            foreach (var module in train.GetModules())
            {
                if (module.Type == ModuleType.Cargo)
                {
                    scoreService.AddScore(module.Score);
                    modulesToDestroy.Add(module);
                    Debug.Log($"💰 Player docked at Tax Station — cargo module sold for {module.Score} points!");
                    continue;
                }
                
                if (module.RequiresRepair)
                {
                    module.Repair();
                    Debug.Log($"🔧 Repaired {module.Type} module");
                }
            }
            
            foreach (var shipModule in modulesToDestroy)
            {
                shipModule.DestroyCar();
            }
        }
    }
}