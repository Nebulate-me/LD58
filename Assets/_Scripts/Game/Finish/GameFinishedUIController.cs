using System;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Game.Finish
{
    public class GameFinishedUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text ratingText;
        [SerializeField] private Image background;

        [Header("Colors")]
        [SerializeField] private Color winColor = new(0.3f, 0.8f, 0.4f, 0.85f);
        [SerializeField] private Color loseColor = new(0.9f, 0.3f, 0.3f, 0.85f);

        private bool shown;

        private void OnEnable()
        {
            SignalsHub.AddListener<GameFinishedSignal>(OnGameFinished);   
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<GameFinishedSignal>(OnGameFinished);   
        }

        private void Start()
        {
            rootGroup.alpha = 0;
        }
        
        private void Update()
        {
            if (shown && Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnGameFinished(GameFinishedSignal signal)
        {
            if (shown) return;
            shown = true;

            rootGroup.gameObject.SetActive(true);
            rootGroup.alpha = 1;
            // background.color = signal.Success ? winColor : loseColor;

            headerText.text = signal.Success ? "🎉 Congratulations!" : "💀 Game Over";
            scoreText.text = $"Score: {signal.FinalScore}";
            ratingText.text = GetRating(signal.FinalScore);

            Time.timeScale = 0f; // pause
        }

        private string GetRating(int score)
        {
            if (score < 100) return "Rating: Novice Taxman";
            if (score < 300) return "Rating: Experienced Collector";
            if (score < 600) return "Rating: Master Auditor";
            return "Rating: Galactic Bureaucrat Supreme";
        }
    }
}
