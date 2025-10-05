using UnityEngine;

namespace _Scripts.Common
{
    public class CommonSettingsProvider : MonoBehaviour, ICommonSettingsProvider
    {
        [Header("Settings")]
        [Tooltip("Extra margin allowed to the right before despawning.")]
        [SerializeField] private float rightMargin = 10f;

        [Tooltip("Extra margin outside the top/bottom/left before despawning.")]
        [SerializeField] private float otherMargins = 2f;
        
        [Tooltip("Spacing between Ship Modules")]
        [SerializeField] private float moduleSpacing = 1.5f;
        
        public float RightMargin => rightMargin;
        public float OtherMargins => otherMargins;
        public float ModuleSpacing => moduleSpacing;
    }
}