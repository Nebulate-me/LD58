using _Scripts.Game.Encounters;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Flow Settings")]
        [Tooltip("Seconds between random encounters")]
        [SerializeField] private float timeBetweenEvents = 30f;
        [SerializeField] private GameInitializer gameInitializer;

        [Header("Prefabs")]
        [SerializeField] private GameObject traderEncounterPrefab;
        [SerializeField] private GameObject asteroidFieldPrefab;
        [SerializeField] private GameObject taxStationPrefab;

        [Inject] private IPrefabPool prefabPool;

        private float eventTimer;
        private EncounterType currentEncounterType = EncounterType.None;
        private GameObject currentEncounter;

        private void Start()
        {
            eventTimer = 0f;
        }

        private void Update()
        {
            eventTimer += Time.deltaTime;

            if (eventTimer >= timeBetweenEvents)
            {
                if (currentEncounter != null)
                {
                    prefabPool.Despawn(currentEncounter);
                    currentEncounter = null;
                }
                TriggerNextEncounter();
                eventTimer = 0f;
            }
        }

        private void TriggerNextEncounter()
        {
            currentEncounterType = (EncounterType)Random.Range(1, 4); // skip None
            Debug.Log($"🚀 Encounter triggered: {currentEncounterType}");

            Vector2 spawnPos = gameInitializer.PlayerPosition + new Vector2(10f, 0f);
            
            currentEncounter = prefabPool.Spawn(asteroidFieldPrefab, spawnPos, Quaternion.identity);

            //
            // switch (currentEncounterType)
            // {
            //     case EncounterType.Trader:
            //         currentEncounter = prefabPool.Spawn(traderEncounterPrefab, spawnPos, Quaternion.identity);
            //         currentEncounter
            //             .GetComponent<TraderEncounterController>()
            //             .SetUp(gameInitializer.PlayerTrain.transform);
            //         break;
            //
            //     case EncounterType.AsteroidField:
            //         currentEncounter = prefabPool.Spawn(asteroidFieldPrefab, spawnPos, Quaternion.identity);
            //         break;
            //
            //     case EncounterType.TaxStation:
            //         currentEncounter = prefabPool.Spawn(taxStationPrefab, spawnPos, Quaternion.identity);
            //         break;
            // }
        }
    }
}