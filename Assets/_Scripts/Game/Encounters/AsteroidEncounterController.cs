using _Scripts.Static;
using Signals;
using UnityEngine;
using Utilities.Prefabs;
using Utilities.RandomService;
using Zenject;

namespace _Scripts.Game.Encounters
{
    public class AsteroidEncounterController : MonoBehaviour, IPoolableResource
    {
        [Header("Asteroid Prefabs")]
        [SerializeField] private GameObject smallAsteroidPrefab;
        [SerializeField] private GameObject largeAsteroidPrefab;

        [Header("Base Settings")]
        [SerializeField] private float baseSpawnInterval = 2f;
        [SerializeField] private int baseMaxActiveAsteroids = 10;
        [SerializeField] private float baseLargeAsteroidChance = 0.25f;
        [SerializeField] private float diagonalChance = 0.15f; // 15% fly diagonally
        [SerializeField] private float diagonalAngleRange = 25f; // degrees from horizontal

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;
        [Inject] private IGameFlowController flowController;

        private float spawnTimer;
        private int currentAsteroids;

        public void OnSpawn()
        {
            SignalsHub.AddListener<AsteroidDestroyedSignal>(OnAsteroidDestroyed);
        }

        public void OnDespawn()
        {
            SignalsHub.RemoveListener<AsteroidDestroyedSignal>(OnAsteroidDestroyed);
            currentAsteroids = 0;
            spawnTimer = 0;
        }

        private void OnAsteroidDestroyed(AsteroidDestroyedSignal _)
        {
            currentAsteroids = Mathf.Max(0, currentAsteroids - 1);
        }

        private void Update()
        {
            spawnTimer += Time.deltaTime;

            float diff = Mathf.Clamp01(flowController.CurrentDifficulty);

            // scale difficulty
            float spawnInterval = Mathf.Lerp(baseSpawnInterval, 0.8f, diff);
            int maxActiveAsteroids = Mathf.RoundToInt(Mathf.Lerp(baseMaxActiveAsteroids, baseMaxActiveAsteroids * 2f, diff));
            float largeAsteroidChance = Mathf.Lerp(baseLargeAsteroidChance, 0.5f, diff);

            if (spawnTimer >= spawnInterval && currentAsteroids < maxActiveAsteroids)
            {
                spawnTimer = 0f;
                SpawnAsteroid(largeAsteroidChance);
            }
        }

        private void SpawnAsteroid(float largeChance)
        {
            bool spawnLarge = randomService.Float(0, 1) < largeChance;
            var prefab = spawnLarge ? largeAsteroidPrefab : smallAsteroidPrefab;

            if (prefab == null)
            {
                Debug.LogWarning("AsteroidEncounter missing prefab!");
                return;
            }

            var spawnY = randomService.Float(-10f, 10f);
            var spawnPos = (Vector2)transform.position + new Vector2(0, spawnY);
            var asteroidObj = prefabPool.Spawn(prefab, spawnPos, Quaternion.identity, transform);

            // direction logic
            Vector2 dir = Vector2.left;
            if (randomService.Float(0, 1) < diagonalChance)
            {
                float angle = randomService.Float(-diagonalAngleRange, diagonalAngleRange);
                dir = Quaternion.Euler(0, 0, angle) * dir;
            }

            if (asteroidObj.TryGetComponent(out AsteroidController controller))
            {
                controller.SetDirection(dir.normalized);
            }

            currentAsteroids++;
        }
    }
}
