using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Game.Finish;
using _Scripts.Ships;
using Signals;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;
using Random = UnityEngine.Random;

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
        [SerializeField] private float totalDuration = 600f; 
        [SerializeField] private float minEventSpacing = 10f;

        [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 0f, 1f, 1f);

        [Header("Level Map")] [SerializeField] private List<LevelEvent> levelMap = new();
        [SerializeField] private bool generateRandomMap = true;
        [SerializeField] private int generatedEventCount = 12;
        [SerializeField] private float timeBetweenEvents = 30f;

        [Header("Prefabs")] [SerializeField] private GameObject traderEncounterPrefab;
        [SerializeField] private GameObject asteroidFieldPrefab;
        [SerializeField] private GameObject pirateEncounterPrefab;
        [SerializeField] private GameObject taxStationPrefab;

        [Header("Refs")] [SerializeField] private GameInitializer gameInitializer;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IScoreService scoreService;

        private int nextEventIndex;
        private bool levelCompletedRaised;

        public float CurrentDifficulty { get; private set; }
        public float ElapsedTime { get; private set; }

        public float TotalDuration { get; private set; }
        public bool IsLevelComplete => ElapsedTime >= Mathf.Max(0.0001f, TotalDuration);

        public event Action OnLevelCompleted;

        private void Start()
        {
            if (generateRandomMap)
                GenerateLevelMap();

            levelMap.Sort((a, b) => a.time.CompareTo(b.time));
            TotalDuration = levelMap.Count > 0 ? levelMap[^1].time : 0f;

            nextEventIndex = 0;
            ElapsedTime = 0f;
            levelCompletedRaised = false;
        }

        private void Update()
        {
            ElapsedTime += Time.deltaTime;

            // Difficulty 0..1
            var t = Mathf.Clamp01(timeToReachMaxDifficulty <= 0 ? 1f : ElapsedTime / timeToReachMaxDifficulty);
            CurrentDifficulty = difficultyCurve.Evaluate(t);

            // Trigger scheduled encounters
            while (nextEventIndex < levelMap.Count && ElapsedTime >= levelMap[nextEventIndex].time)
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

            // --- 0️⃣ Ensure an early Trader event right after start ---
            float earlyTraderTime = Mathf.Max(minEventSpacing * 1.5f, 5f);
            AddEventSafe(earlyTraderTime, EncounterType.Trader);

            // --- 1️⃣ Fixed Tax Station positions ---
            float[] taxFractions = { 0.10f, 0.25f, 0.45f, 0.70f, 0.95f };
            List<float> taxTimes = taxFractions.Select(f => f * totalDuration).ToList();

            foreach (float t in taxTimes)
                AddEventSafe(t, EncounterType.TaxStation);
            
            // --- 1️⃣ Guaranteed early Pirate encounter after first Tax Station ---
            if (taxTimes.Count > 0)
            {
                float firstTax = taxTimes[0];
                float pirateAfterTax = firstTax + Mathf.Max(minEventSpacing, 5f);
                AddEventSafe(pirateAfterTax, EncounterType.PirateAttack);
            }

            // --- 2️⃣ Fill gaps between tax stations with Trader & Pirate encounters ---
            for (var i = 0; i < taxTimes.Count - 1; i++)
            {
                var start = taxTimes[i];
                var end = taxTimes[i + 1];

                var encountersInSegment = Random.Range(2, 4);
                for (var e = 0; e < encountersInSegment; e++)
                {
                    var time = RandomBetweenSafe(start + minEventSpacing, end - minEventSpacing);
                    var type = Random.value > 0.5f
                        ? EncounterType.Trader
                        : EncounterType.PirateAttack;

                    AddEventSafe(time, type);
                }
            }

            // --- 3️⃣ Independent asteroid fields (free-floating complexity) ---
            var asteroidCount = Mathf.RoundToInt(totalDuration / 25f); // roughly one per 25s
            for (var i = 0; i < asteroidCount; i++)
            {
                var time = RandomBetweenSafe(0f, totalDuration);
                AddEventSafe(time, EncounterType.AsteroidField);
            }

            // --- 4️⃣ Pirate "boss wave" near end ---
            var endStart = totalDuration * 0.8f;
            var finalPirates = Random.Range(2, 4);
            for (var i = 0; i < finalPirates; i++)
            {
                var time = RandomBetweenSafe(endStart, totalDuration - minEventSpacing);
                AddEventSafe(time, EncounterType.PirateAttack);
            }

            // --- 5️⃣ Sort chronologically ---
            levelMap.Sort((a, b) => a.time.CompareTo(b.time));

            Debug.Log($"Structured level map generated ({levelMap.Count} events, duration {totalDuration}s).");
        }

// --- Helper: safely adds an event respecting minimum spacing ---
        private void AddEventSafe(float time, EncounterType type)
        {
            if (levelMap.Count == 0)
            {
                levelMap.Add(new LevelEvent { time = time, encounter = type });
                return;
            }

            // Sort first to check nearest neighbors
            levelMap.Sort((a, b) => a.time.CompareTo(b.time));

            bool adjusted;
            int guard = 0;
            do
            {
                adjusted = false;
                foreach (var ev in levelMap)
                {
                    if (Mathf.Abs(ev.time - time) < minEventSpacing)
                    {
                        // too close — move forward slightly
                        time = ev.time + minEventSpacing;
                        adjusted = true;
                    }
                }

                // prevent infinite loops
                guard++;
                if (guard > 50) break;

            } while (adjusted && time < totalDuration - minEventSpacing);

            // clamp to duration limit
            time = Mathf.Min(time, totalDuration - minEventSpacing);

            levelMap.Add(new LevelEvent { time = time, encounter = type });
        }

// --- Helper: returns random time respecting minEventSpacing margins ---
        private float RandomBetweenSafe(float start, float end)
        {
            if (end - start < minEventSpacing)
                return (start + end) / 2f;
            return Random.Range(start, end);
        }


        private void TriggerEncounter(LevelEvent levelEvent)
        {
            var spawnPos = gameInitializer.PlayerPosition + new Vector2(10f, 0);
            var prefab = levelEvent.encounter switch
            {
                EncounterType.Trader => traderEncounterPrefab,
                EncounterType.AsteroidField => asteroidFieldPrefab,
                EncounterType.PirateAttack => pirateEncounterPrefab,
                EncounterType.TaxStation => taxStationPrefab,
                _ => null
            };

            if (prefab != null)
                prefabPool.Spawn(prefab, spawnPos, Quaternion.identity);

            Debug.Log(
                $"🚀 Encounter triggered: {levelEvent.encounter} at {ElapsedTime:F1}s (Diff {CurrentDifficulty:F2})");
        }

        public IReadOnlyList<LevelEvent> GetLevelMap()
        {
            return levelMap;
        }

        public bool TryGetPlayer(out TrainController playerShip)
        {
            var playerShipExists = gameInitializer.PlayerShip != null;
            playerShip = gameInitializer.PlayerShip;

            return playerShipExists;
        }
    }
}