using UnityEngine;

namespace _Scripts.Visuals
{
    public class ScrollingBackground : MonoBehaviour, IScrollingBackground
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scrollSpeed = 2f;
        
        private Camera mainCamera; // TODO: Inject
        
        private float spriteWidth;
        

        void Start()
        {
            mainCamera = Camera.main;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            spriteWidth = spriteRenderer.bounds.size.x;
        }

        void Update()
        {
            // Move left at constant speed
            transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

            // If completely offscreen to the left, move to the right end
            if (transform.position.x < mainCamera.transform.position.x - spriteWidth * 1f)
            {
                Vector3 newPos = transform.position;
                newPos.x += spriteWidth * 2f; // jump over the neighbor
                transform.position = newPos;
            }
        }

        // Public control
        public void SetSpeed(float newSpeed) => scrollSpeed = newSpeed;
        public void Stop() => scrollSpeed = 0f;
    }
}