using UnityEngine;
using Utilities.Prefabs;

namespace _Scripts.Common
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour, IPoolableResource
    {
        [Header("Colors")]
        [SerializeField] private float flashDuration = 0.1f;
        
        [SerializeField] private Color fullHealthColor = Color.white;
        [SerializeField] private Color lowHealthColor = new(0.6f, 0.3f, 0.3f, 1f);
        [SerializeField] private GameObject smokePrefab;
        
        private GameObject _smokeInstance;
        private bool _smokeActive;

        private Health _health;
        private SpriteRenderer _sprite;
        private Color _baseColor;
        private float _flashTimer;
        private Material _mat;
        
        private static readonly int FlashProp = Shader.PropertyToID("_FlashAmount");
        private static readonly int DarkenProp = Shader.PropertyToID("_DarkenAmount");

        public void OnSpawn()
        {
            _health.OnDamaged += StartFlash;
            _health.OnDeath += OnDeath;
            
            _health.OnDamaged += UpdateDamageTint;
            _health.OnHealed += UpdateDamageTint;
            _health.OnDeath += UpdateDamageTint;
        }

        public void OnDespawn()
        {
            _health.OnDamaged -= StartFlash;
            _health.OnDeath -= OnDeath;
            
            _health.OnDamaged -= UpdateDamageTint;
            _health.OnHealed -= UpdateDamageTint;
            _health.OnDeath -= UpdateDamageTint;
        }


        private void Awake()
        {
            _health = GetComponent<Health>();
            _sprite = GetComponent<SpriteRenderer>();
            _mat = _sprite.material;
        }

        private void Update()
        {
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_flashTimer / flashDuration);
                _mat.SetFloat(FlashProp, t);
                _mat.SetFloat(DarkenProp, 0);
            }
            else
            {
                _mat.SetFloat(FlashProp, 0f);
                var hpRatio = 1 - Mathf.Clamp01((float)_health.CurrentHealth / _health.MaxHealth);
                _mat.SetFloat(DarkenProp, hpRatio);
            }
        }

        private void StartFlash(Health health)
        {
            StartFlash();
        }
        
        private void StartFlash()
        {
            Debug.Log($"StartFlash to {flashDuration}");
            _flashTimer = flashDuration;
        }

        private void OnDeath(Health health)
        {
            OnDeath();
        }
        private void OnDeath()
        {
            _sprite.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
        
        private void UpdateDamageTint(Health obj)
        {
            UpdateDamageTint();
        }

        private void UpdateDamageTint()
        {
            float hpRatio = Mathf.Clamp01((float)_health.CurrentHealth / _health.MaxHealth);
            _sprite.color = Color.Lerp(Color.gray, _baseColor, hpRatio);
        }
    }
}