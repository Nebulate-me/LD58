using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Ships.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

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

        public bool IsPlayerControlled
        {
            get => isPlayerControlled;
            set => isPlayerControlled = value;
        }

        void Update()
        {
            if (head == null) return;

            // 1. Update head
            head.UpdateHead(isPlayerControlled);

            // 2. Update followers
            for (int i = 1; i < modules.Count; i++)
            {
                modules[i].Follow(modules[i - 1].transform.position, spacing, moveSpeed);
            }
        }

        public void AddModule(ShipModule newModule)
        {
            modules.Add(newModule);
            newModule.AssignToTrain(this);

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
                // 🚨 No locomotives left
                if (isPlayerControlled)
                {
                    Debug.Log("Game Over – Player lost all locomotives!");
                    // TODO: trigger game over
                }
                else
                {
                    Debug.Log("AI train disabled – no locomotives left");
                    // TODO: make this train inert
                }
            }
        }

        public IReadOnlyList<ShipModule> GetModules() => modules;

        public IReadOnlyList<SpriteRenderer> GetModuleSpriteRenderers() => modules.Select(module => module.SpriteRenderer).ToList();
    }
}