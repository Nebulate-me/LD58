using _Scripts.Common;
using _Scripts.Ships.Modules;
using UnityEngine;

namespace _Scripts.Static
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(Health))]
    public class AsteroidController : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Rigidbody2D rigidBody;
        
        [Header("Stats")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private Vector2 moveDir = Vector2.left;

        private void Awake()
        {
            if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
            if (health == null) health = GetComponent<Health>();
        }

        private void Update()
        {
            transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var module = other.GetComponent<ShipModule>();
            if (module != null)
            {
                module.Health.Damage(health.CurrentHealth);
                health.Damage(health.CurrentHealth);
            }
        }
    }
}