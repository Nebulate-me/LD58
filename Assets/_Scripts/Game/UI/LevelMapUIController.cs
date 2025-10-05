using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Prefabs;
using Zenject;

namespace _Scripts.Game.UI
{
    public class LevelMapUIController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform timelineContainer;   // the full bar area (background)
        [SerializeField] private Image progressBar;                 // sliced foreground fill
        [SerializeField] private RectTransform progressMarker;      // player marker
        [SerializeField] private GameObject levelEncounterIconPrefab;

        [Header("Icons")]
        [SerializeField] private EncounterTypeToIconDictionary encounterIcons = new();

        [Header("Appearance")]
        [SerializeField] private float iconSize = 32f;
        [SerializeField] private float iconYOffset = -8f; // small offset above/below the bar

        [Inject] private IPrefabPool prefabPool;
        [Inject] private IGameFlowController gameFlow;

        private readonly List<Image> _icons = new();
        private float _totalDuration;
        private bool _built;

        private RectTransform ProgressFillRT => progressBar != null ? progressBar.rectTransform : null;

        private void OnEnable()
        {
            if (gameFlow != null)
                gameFlow.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            if (gameFlow != null)
                gameFlow.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void Start()
        {
            TryBuildTimeline();
        }

        private void Update()
        {
            // Rebuild if rect changed or not built yet (first frame after layout)
            if (!_built || timelineContainer.hasChanged)
            {
                timelineContainer.hasChanged = false;
                Rebuild();
            }

            UpdateProgress();
            
            foreach (Transform child in timelineContainer)
            {
                if (child.TryGetComponent(out LevelMapEncounterIcon icon))
                    icon.UpdateVisual(gameFlow.ElapsedTime);
            }

        }

        private void TryBuildTimeline()
        {
            var map = gameFlow.GetLevelMap();
            if (map == null || map.Count == 0) return;

            _totalDuration = gameFlow.TotalDuration; // last event time from flow
            BuildIcons();
            _built = true;
        }

        private void Rebuild()
        {
            ClearIcons();
            TryBuildTimeline();
        }

        private float GetTimelineWidth()
        {
            if (timelineContainer == null) return 0f;
            return Mathf.Max(0f, timelineContainer.rect.width);
        }

        private void BuildIcons()
        {
            var map = gameFlow.GetLevelMap();
            if (map == null || map.Count == 0 || timelineContainer == null) return;

            float width = GetTimelineWidth();

            foreach (var ev in map)
            {
                var iconGo = prefabPool.Spawn(levelEncounterIconPrefab, timelineContainer);
                var rt = iconGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(iconSize, iconSize);

                float x = (_totalDuration > 0f) ? (ev.time / _totalDuration) * width : 0f;
                rt.anchoredPosition = new Vector2(x, iconYOffset);

                var iconController = iconGo.GetComponent<LevelMapEncounterIcon>();
                if (iconController != null)
                {
                    iconController.SetUp(ev.encounter, ev.time);
                    var img = iconGo.GetComponent<Image>();
                    img.sprite = encounterIcons.GetValueOrDefault(ev.encounter);
                }

                _icons.Add(iconGo.GetComponent<Image>());
            }
        }


        private void UpdateProgress()
        {
            if (_totalDuration <= 0f) return;

            float width = GetTimelineWidth();
            float t = Mathf.Clamp01(gameFlow.ElapsedTime / _totalDuration);
            float x = t * width;

            if (ProgressFillRT != null)
            {
                ProgressFillRT.anchorMin = new Vector2(0f, ProgressFillRT.anchorMin.y);
                ProgressFillRT.anchorMax = new Vector2(0f, ProgressFillRT.anchorMax.y);
                ProgressFillRT.pivot     = new Vector2(0f, 0.5f);

                ProgressFillRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            }

            // move the marker left -> right
            if (progressMarker != null)
            {
                progressMarker.anchorMin = new Vector2(0f, progressMarker.anchorMin.y);
                progressMarker.anchorMax = new Vector2(0f, progressMarker.anchorMax.y);
                progressMarker.pivot     = new Vector2(0.5f, progressMarker.pivot.y);

                var pos = progressMarker.anchoredPosition;
                pos.x = x;
                progressMarker.anchoredPosition = pos;
                
                progressMarker.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 4f) * 0.1f);
            }
        }

        private void HandleLevelCompleted()
        {
            // Snap UI to the end when level is done
            UpdateProgress();
            // Optional: tint remaining icons or show “WIN!”
        }

        private void ClearIcons()
        {
            for (int i = 0; i < _icons.Count; i++)
            {
                if (_icons[i] != null)
                    Destroy(_icons[i].gameObject);
            }
            _icons.Clear();
        }
    }
}
