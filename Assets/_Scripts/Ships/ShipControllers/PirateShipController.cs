using System.Collections.Generic;
using System.Linq;
using _Scripts.Ships.Modules;
using UnityEngine;

namespace _Scripts.Ships.ShipControllers
{
    [RequireComponent(typeof(TrainController))]
    public class PirateShipController : MonoBehaviour
    {
        [Header("Behaviour Settings")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float approachDistance = 6f;
        [SerializeField] private float evadeRadius = 3f;
        [SerializeField] private float detectionRadius = 8f;
        [SerializeField] private float lateralStrafeSpeed = 1.5f;

        [Header("Weights")]
        [SerializeField] private float pursueWeight = 1f;
        [SerializeField] private float evadeWeight = 2.5f;
        [SerializeField] private float wanderWeight = 0.5f;

        private TrainController _train;
        private Transform _player;
        private Vector2 _velocity;
        private Vector2 _wanderTarget;

        private void Awake()
        {
            _train = GetComponent<TrainController>();
        }

        public void Initialize(Transform player)
        {
            _player = player;
            _wanderTarget = Random.insideUnitCircle.normalized;
        }

        private void Update()
        {
            if (_player == null) return;

            Vector2 desiredDir = Vector2.zero;

            // 1️⃣ Pursue player
            Vector2 toPlayer = (Vector2)(_player.position - transform.position);
            float distToPlayer = toPlayer.magnitude;
            if (distToPlayer > approachDistance)
            {
                desiredDir += toPlayer.normalized * pursueWeight;
            }
            else
            {
                // orbit around player
                Vector2 perpendicular = Vector2.Perpendicular(toPlayer).normalized;
                desiredDir += perpendicular * lateralStrafeSpeed;
            }

            // 2️⃣ Evade asteroids / projectiles
            var threats = FindThreats();
            foreach (var threat in threats)
            {
                Vector2 away = (Vector2)(transform.position - threat.position);
                float dist = away.magnitude;
                if (dist < evadeRadius)
                    desiredDir += away.normalized * (evadeWeight / Mathf.Max(0.1f, dist));
            }

            // 3️⃣ Add wandering when idle
            if (desiredDir.sqrMagnitude < 0.1f)
            {
                _wanderTarget += Random.insideUnitCircle * 0.3f;
                desiredDir += _wanderTarget.normalized * wanderWeight;
            }

            // Normalize
            if (desiredDir.sqrMagnitude > 0.001f)
                desiredDir.Normalize();

            // 4️⃣ Move train’s head manually (since AI train won’t read player input)
            MoveTrain(desiredDir);
        }

        private void MoveTrain(Vector2 direction)
        {
            // simple manual movement for head
            ShipModule head = _train.GetModules().FirstOrDefault();
            if (head == null) return;

            head.transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
        }

        private IEnumerable<Transform> FindThreats()
        {
            // Find all projectiles or asteroids near the pirate
            foreach (var proj in FindObjectsOfType<ProjectileController>())
            {
                if (proj == null) continue;
                if (proj.transform == null) continue;
                // ignore friendly (enemy projectiles)
                if (!proj.gameObject.activeInHierarchy) continue;
                if (Vector2.Distance(transform.position, proj.transform.position) < detectionRadius)
                    yield return proj.transform;
            }

            foreach (var asteroid in GameObject.FindGameObjectsWithTag("Asteroid"))
            {
                if (Vector2.Distance(transform.position, asteroid.transform.position) < detectionRadius)
                    yield return asteroid.transform;
            }
        }
    }
}
