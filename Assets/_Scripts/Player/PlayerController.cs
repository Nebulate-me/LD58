using UnityEngine;

namespace _Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        
        private Rigidbody2D rb;
        private Camera cam;
        private Collider2D col;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            cam = Camera.main;
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            rb.velocity = new Vector2(moveX, moveY) * moveSpeed;

            ClampToScreen();
        }

        private void ClampToScreen()
        {
            Bounds bounds = col.bounds;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Vector3 pos = transform.position;

            // Clamp with collider extents
            pos.x = Mathf.Clamp(pos.x, 
                cam.transform.position.x - halfWidth + bounds.extents.x, 
                cam.transform.position.x + halfWidth - bounds.extents.x);

            pos.y = Mathf.Clamp(pos.y, 
                cam.transform.position.y - halfHeight + bounds.extents.y, 
                cam.transform.position.y + halfHeight - bounds.extents.y);

            transform.position = pos;
        }
    }
}