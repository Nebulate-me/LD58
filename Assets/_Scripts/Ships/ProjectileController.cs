using _Scripts.Common;
using _Scripts.Ships.Modules;
using _Scripts.Static;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;
using Zenject.Asteroids;

namespace _Scripts.Ships
{
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 4f;
        [SerializeField] private int damage = 1;

        private bool _isPlayer;
        private float _speed;
        private float _timer;
        
        [Inject] private IPrefabPool prefabPool;
        public bool IsPlayerProjectile =>  _isPlayer;

        public void Initialize(bool fromPlayer, float speed)
        {
            _isPlayer = fromPlayer;
            _speed = speed;
            _timer = 0f;
        }

        private void Update()
        {
            transform.Translate(Vector3.right * _speed * Time.deltaTime);
            _timer += Time.deltaTime;
            if (_timer >= lifeTime)
            {
                prefabPool.Despawn(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var asteroid = other.GetComponent<AsteroidController>();
            if (asteroid != null)
            {
                Damage(asteroid.Health);
                return;
            }
            
            var module = other.GetComponent<ShipModule>();
            if (module != null &&
                module.Train != null &&
                module.Train.IsPlayerControlled != _isPlayer)
            {
                Damage(module.Health);
                return;
            }
        }

        private void Damage(IHealth otherHealth)
        {
            otherHealth.Damage(damage);
            prefabPool.Despawn(gameObject);
        }
    }
}