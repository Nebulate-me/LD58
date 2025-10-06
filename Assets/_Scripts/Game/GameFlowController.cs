using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Game.Finish;
using _Scripts.Ships;
using Signals;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game
{
    [Serializable]
    public class LevelEvent
    {
        public float time;
        public EncounterType encounter;
    }

    public class GameFlowController : MonoBehaviour, IGameFlowController
    {
        [Header("Difficulty Growth")]
        [SerializeField] private float timeToReachMaxDifficulty = 600f;
        [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 0f, 1f, 1f);

        [Header("Level Map")]
        [SerializeField] private List<LevelEvent> levelMap = new();
        [SerializeField] private bool generateRandomMap = true;
        [SerializeField] private int generatedEventCount = 12;
        [SerializeField] private float timeBetweenEvents = 30f;

        [Header("Prefabs")]
        [SerializeField] private GameObject traderEncounterPrefab;
        [SerializeField] private GameObject asteroidFieldPrefab;
        [SerializeField] private GameObject pirateEncounterPrefab;
        [SerializeField] private GameObject taxStationPrefab;

        [Header("Refs")]
        [SerializeField] private GameInitializer gameInitializer;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IScoreService scoreService;

        private float elapsedTime;
        private int nextEventIndex;
        private bool levelCompletedRaised;

        public float CurrentDifficulty { get; private set; }
        public float ElapsedTime => elapsedTime;
        public float TotalDuration { get; private set; }
        public bool IsLevelComplete => elapsedTime >= Mathf.Max(0.0001f, TotalDuration);

        public event Action OnLevelCompleted;

        private void Start()
        {
            if (generateRandomMap)
                GenerateLevelMap();

            levelMap.Sort((a, b) => a.time.CompareTo(b.time));
            TotalDuration = levelMap.Count > 0 ? levelMap[^1].time : 0f;

            nextEventIndex = 0;
            elapsedTime = 0f;
            levelCompletedRaised = false;
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            // Difficulty 0..1
            float t = Mathf.Clamp01(timeToReachMaxDifficulty <= 0 ? 1f : elapsedTime / timeToReachMaxDifficulty);
            CurrentDifficulty = difficultyCurve.Evaluate(t);

            // Trigger scheduled encounters
            while (nextEventIndex < levelMap.Count && elapsedTime >= levelMap[nextEventIndex].time)
            {
                TriggerEncounter(levelMap[nextEventIndex]);
                nextEventIndex++;
            }

            if (!levelCompletedRaised && IsLevelComplete)
            {
                levelCompletedRaised = true;
                OnLevelCompleted?.Invoke();
                
                SignalsHub.DispatchAsync(new GameFinishedSignal(true, scoreService.CurrentScore));
                Debug.Log("🏁 Level completed — Game Finished (Win)");
            }
        }

        private void GenerateLevelMap()
        {
            levelMap.Clear();

            // --- Encounter weights ---
            float asteroidWeight = 0.3f;
            float traderWeight   = 0.25f;
            float pirateWeight   = 0.35f;
            float taxWeight      = 0.10f;

            // total = 1.0, but we normalize just in case
            float total = asteroidWeight + traderWeight + pirateWeight + taxWeight;

            // --- Pre-determine tax station times (rarer & spaced further apart) ---
            int taxStationsToPlace = Mathf.Clamp(Mathf.RoundToInt(generatedEventCount / 4f), 3, 4);
            List<float> taxTimes = new();
            float levelLength = generatedEventCount * timeBetweenEvents;
            for (int i = 0; i < taxStationsToPlace; i++)
            {
                float t = (i + 1) / (float)(taxStationsToPlace + 1) * levelLength;
                taxTimes.Add(t + UnityEngine.Random.Range(-10f, 10f));
            }

            // --- Main generation loop ---
            for (int i = 0; i < generatedEventCount; i++)
            {
                float time = i * timeBetweenEvents + UnityEngine.Random.Range(-5f, 5f);

                EncounterType type;

                // Force tax station at certain pre-set positions
                if (taxTimes.Any(t => Mathf.Abs(t - time) < timeBetweenEvents * 0.5f))
                {
                    type = EncounterType.TaxStation;
                }
                else
                {
                    // Weighted random draw
                    float r = UnityEngine.Random.Range(0f, total);
                    if (r < asteroidWeight) type = EncounterType.AsteroidField;
                    else if (r < asteroidWeight + traderWeight) type = EncounterType.Trader;
                    else if (r < asteroidWeight + traderWeight + pirateWeight) type = EncounterType.PirateAttack;
                    else type = EncounterType.TaxStation;
                }

                levelMap.Add(new LevelEvent { time = Mathf.Max(0, time), encounter = type });
            }

            // Sort events by time
            levelMap.Sort((a, b) => a.time.CompareTo(b.time));
        }


        private void TriggerEncounter(LevelEvent levelEvent)
        {
            Vector2 spawnPos = gameInitializer.PlayerPosition + new Vector2(10f, 0);
            GameObject prefab = levelEvent.encounter switch
            {
                EncounterType.Trader        => traderEncounterPrefab,
                EncounterType.AsteroidField => asteroidFieldPrefab,
                EncounterType.PirateAttack  => pirateEncounterPrefab,
                EncounterType.TaxStation    => taxStationPrefab,
                _ => null
            };

            if (prefab != null)
                prefabPool.Spawn(prefab, spawnPos, Quaternion.identity);

            Debug.Log($"🚀 Encounter triggered: {levelEvent.encounter} at {elapsedTime:F1}s (Diff {CurrentDifficulty:F2})");
        }

        public IReadOnlyList<LevelEvent> GetLevelMap() => levelMap;
        public bool TryGetPlayer(out TrainController playerShip)
        {
            var playerShipExists = gameInitializer.PlayerShip != null;
            playerShip = gameInitializer.PlayerShip;
            
            return playerShipExists;
        }
    }
}
