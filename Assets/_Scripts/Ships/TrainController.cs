using System.Collections.Generic;
using System.Linq;
using _Scripts.Common;
using _Scripts.Game;
using _Scripts.Game.Finish;
using _Scripts.Ships.Modules;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Ships
{
    public class TrainController : MonoBehaviour
    {
        [Header("Train Settings")]
        [SerializeField] private float spacing = 1.0f;
        [SerializeField] private float moveSpeed = 10f;

        [ShowInInspector, ReadOnly] private List<ShipModule> modules = new();
        [ShowInInspector, ReadOnly] private ShipModule head;
        [ShowInInspector, ReadOnly] private bool isPlayerControlled = false;
        
        [Inject] private IPrefabPool prefabPool;
        [Inject] private ICommonSettingsProvider commonSettingsProvider;
        [Inject] private IModuleRegistry moduleRegistry;
        [Inject] private IScoreService scoreService;

        public bool IsPlayerControlled
        {
            get => isPlayerControlled;
            set => isPlayerControlled = value;
        }

        public bool HasLocomotive => head != null;
        public Transform Head => head?.transform ?? transform;
        public FacingDirection Facing { get; private set; } = FacingDirection.Left;

        void Update()
        {
            if (head == null) return;
            
            head.UpdateHead(isPlayerControlled);
            
            for (var i = 1; i < modules.Count; i++)
            {
                modules[i].Follow(modules[i - 1].transform.position, spacing, moveSpeed);
            }
        }

        public void AddModule(ShipModule newModule)
        {
            modules.Add(newModule);

            if (head == null && newModule.Type == ModuleType.Locomotive)
            {
                head = newModule;
            }
        }

        public void RemoveModule(ShipModule moduleToDelete)
        {
            int index = modules.IndexOf(moduleToDelete);
            if (index == -1) return;

            bool wasHead = (moduleToDelete == head);

            modules.RemoveAt(index);

            if (wasHead)
            {
                PromoteNewHead();
            }
        }

        private void PromoteNewHead()
        {
            head = null;

            foreach (var car in modules)
            {
                if (car.Type == ModuleType.Locomotive)
                {
                    head = car;
                    break;
                }
            }

            if (head == null)
            {
                for (var index = modules.Count - 1; index >= 0; index--)
                {
                    var shipModule = modules[index];
                    shipModule.DetachFromShip();
                }

                // 🚨 No locomotives left
                if (isPlayerControlled)
                {
                    var score = scoreService.CurrentScore;
                    SignalsHub.DispatchAsync(new GameFinishedSignal(false, score));
                    Debug.Log("💀 Player train destroyed — Game Over");
                }
                else
                {
                    Debug.Log("AI train disabled – no locomotives left");
                    // TODO: make this train inert
                }
            }
        }
        
        public void AssembleShip(ShipConfiguration shipConfiguration, Vector3 startPos)
        {
            Facing = shipConfiguration.Facing;
            var currentModulePosition = startPos;
            var positionIncrement = (int) shipConfiguration.Facing * -commonSettingsProvider.ModuleSpacing;
            
            foreach (var moduleConfig in shipConfiguration.Modules)
            {
                var module = prefabPool
                    .Spawn(moduleConfig.Prefab, currentModulePosition, Quaternion.identity, transform)
                    .GetComponent<ShipModule>();
                currentModulePosition += Vector3.right * positionIncrement;
                module.SetFacing(shipConfiguration.Facing);

                module.AttachToShip(this);
            }
        }

        public IReadOnlyList<ShipModule> GetModules() => modules;

        public IReadOnlyList<SpriteRenderer> GetModuleSpriteRenderers() => modules.Select(module => module.SpriteRenderer).ToList();
    }
}