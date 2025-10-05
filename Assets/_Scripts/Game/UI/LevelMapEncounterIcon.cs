using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Game.UI
{
    /// <summary>
    /// Handles visual state (idle, active, completed) and glow for a single encounter icon on the level map.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LevelMapEncounterIcon : MonoBehaviour
    {
        [SerializeField] private float upcomingEventInterval = 10f;
        [Header("Colors")]
        [SerializeField] private Color upcomingColor = new(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color completedColor = new(0.6f, 0.6f, 0.6f, 0.6f);

        [Header("Glow")]
        [SerializeField] private float glowSpeed = 3f;
        [SerializeField] private float glowIntensity = 0.2f;
        
        private EncounterType encounterType;
        private float encounterTime;
        private Image _image;
        private bool _isActive;
        private bool _isCompleted;

        private void Awake()
        {
            _image = GetComponent<Image>();
            SetUpcoming();
        }

        public void SetUp(EncounterType eType, float eTime)
        {
            encounterType = eType;
            encounterTime = eTime;
        }

        public void UpdateVisual(float elapsedTime)
        {
            if (_isCompleted) return;

            if (elapsedTime >= encounterTime)
            {
                // mark completed
                _isCompleted = true;
                _isActive = false;
                SetCompleted();
                return;
            }

            // if this is the next upcoming event (active window)
            float timeToEvent = encounterTime - elapsedTime;
            if (timeToEvent <= upcomingEventInterval && !_isActive) // start glowing 5s before
            {
                _isActive = true;
                SetActive();
            }
            else if (timeToEvent > upcomingEventInterval && _isActive)
            {
                _isActive = false;
                SetUpcoming();
            }

            if (_isActive)
                Pulse();
        }

        private void SetUpcoming() => _image.color = upcomingColor;
        private void SetActive() => _image.color = activeColor;
        private void SetCompleted() => _image.color = completedColor;

        private void Pulse()
        {
            transform.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 4f) * 0.1f);
        }
    }
}
