using System.Collections.Generic;
using _Scripts.Ships;
using _Scripts.Ships.Modules;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game.Encounters
{
    public class TraderEncounterController : MonoBehaviour, IPoolableResource
    {
        [Header("Encounter Setup")]
        [SerializeField] private GameObject traderPrefab;

        [SerializeField] private float horizontalOffset = 10f;
        [SerializeField] private float verticalSpread = 4f;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IModuleRegistry moduleRegistry;
        
        private readonly List<GameObject> traders = new List<GameObject>();

        public void SetUp(Transform player)
        {
            // Randomize spawn sides (left/right)
            bool firstOnLeft = Random.value > 0.5f;

            // Spawn Trader A
            Vector2 traderAPos = new Vector2(player.position.x + (firstOnLeft ? -horizontalOffset : horizontalOffset),
                player.position.y + Random.Range(-verticalSpread, verticalSpread));
            var traderAObj = prefabPool.Spawn(traderPrefab, traderAPos, Quaternion.identity, transform);
            var traderA = traderAObj.GetComponent<TraderTrainController>();
            traderA.SetUp(player);
            traders.Add(traderA.gameObject);

            // Spawn Trader B on the opposite side
            Vector2 traderBPos = new Vector2(player.position.x + (firstOnLeft ? horizontalOffset : -horizontalOffset),
                player.position.y + Random.Range(-verticalSpread, verticalSpread));
            var traderBObj = prefabPool.Spawn(traderPrefab, traderBPos, Quaternion.identity, transform);
            var traderB = traderBObj.GetComponent<TraderTrainController>();
            traderB.SetUp(player);
            traders.Add(traderB.gameObject);

            Debug.Log("🚂 Trader Encounter spawned two traders on opposite sides!");
        }

        public void OnSpawn()
        {
            
        }

        public void OnDespawn()
        {
            foreach (var trader in traders)
            {
                prefabPool.Despawn(trader);
            }
        }
    }
}