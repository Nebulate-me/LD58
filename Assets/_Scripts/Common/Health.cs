using System;
using _Scripts.Utils.AudioTool.Sounds;
using Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Common
{
    public class Health : MonoBehaviour, IHealth, IPoolableResource
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private bool despawnOnDeath = false;
        [SerializeField] private bool playExplosionSoundOnDeath = true;

        public int MaxHealth => maxHealth;
        [ShowInInspector, ReadOnly] public int CurrentHealth { get; private set; }
        
        [Inject] private IPrefabPool prefabPool;

        public bool IsDead { get; private set; } = false;
        public bool IsAlive => CurrentHealth > 0;

        public event Action<Health> OnDamaged;
        public event Action<Health> OnHealed;
        public event Action<Health> OnDeath;

        public void SetUp(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            CurrentHealth = newMaxHealth;
        }

        public void Damage(int amount)
        {
            if (amount <= 0 || !IsAlive) return;

            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);

            Debug.Log($"{name} took damage, HP: {CurrentHealth}/{maxHealth}");
            OnDamaged?.Invoke(this);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive) return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);

            OnHealed?.Invoke(this);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            
            if (playExplosionSoundOnDeath) SignalsHub.DispatchAsync(new PlaySoundSignal {Name = SoundName.Explosion});
            
            OnDeath?.Invoke(this);
            
            if (despawnOnDeath) prefabPool.Despawn(gameObject);
        }

        public void OnSpawn()
        {
            CurrentHealth = maxHealth;
        }

        public void OnDespawn()
        {
            IsDead = false;
        }
    }
}