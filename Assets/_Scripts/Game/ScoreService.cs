using TMPro;
using UnityEngine;

namespace _Scripts.Game
{
    public class ScoreService : MonoBehaviour, IScoreService
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI scoreText;

        public int CurrentScore { get; private set; }

        private void Awake()
        {
            ResetScore();
            UpdateUI();
        }

        public void AddScore(int amount)
        {
            if (amount <= 0) return;
            CurrentScore += amount;
            UpdateUI();
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {CurrentScore}";
        }
    }
}