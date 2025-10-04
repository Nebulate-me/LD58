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

        [Header("Spawn Settings")]
        [SerializeField, Range(0, 1)] private float largeAsteroidChance = 0.25f;
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxActiveAsteroids = 12;

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IRandomService randomService;

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

        private void OnAsteroidDestroyed(AsteroidDestroyedSignal asteroidDestroyedSignal)
        {
            currentAsteroids = Mathf.Max(0, currentAsteroids - 1);
        }

        private void Update()
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval && currentAsteroids < maxActiveAsteroids)
            {
                spawnTimer = 0f;
                SpawnAsteroid();
            }
        }

        private void SpawnAsteroid()
        {
            var spawnLarge = randomService.Float(0, 1) < largeAsteroidChance;
            var prefab = spawnLarge ? largeAsteroidPrefab : smallAsteroidPrefab;

            if (prefab == null)
            {
                Debug.LogWarning("AsteroidSpawner missing prefab reference!");
                return;
            }

            var spawnPos = (Vector2) transform.position + new Vector2(0, randomService.Float(-spawnRadius, spawnRadius));
            var asteroidObj = prefabPool.Spawn(prefab, spawnPos, Quaternion.identity, transform);

            var asteroid = asteroidObj.GetComponent<AsteroidController>();
            if (asteroid == null)
            {
                Debug.LogWarning("Asteroid prefab missing AsteroidController component!");
                return;
            }

            /*// Assign type-specific stats
            if (spawnLarge)
            {
                asteroid.Initialize(largeAsteroidHealth, largeAsteroidSpeed, this);
            }
            else
            {
                asteroid.Initialize(smallAsteroidHealth, smallAsteroidSpeed, this);
            }*/

            currentAsteroids++;
        }
    }
}
