using System;
using System.Collections.Generic;
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

            // Win condition
            if (!levelCompletedRaised && IsLevelComplete)
            {
                levelCompletedRaised = true;
                OnLevelCompleted?.Invoke();
                Debug.Log("🏁 Level completed!");
            }
        }

        private void GenerateLevelMap()
        {
            levelMap.Clear();

            for (int i = 0; i < generatedEventCount; i++)
            {
                float time = i * timeBetweenEvents + UnityEngine.Random.Range(-5f, 5f);
                EncounterType type = (EncounterType)UnityEngine.Random.Range(1, 5); // 1..4
                levelMap.Add(new LevelEvent { time = Mathf.Max(0, time), encounter = type });
            }
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
    }
}
